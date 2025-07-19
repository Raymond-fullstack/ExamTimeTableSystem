using ExamTimeTable.Data;
using ExamTimeTable.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamTimeTable.Services
{
    public class ExamGenerationService : IExamGenerationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExamGenerationService> _logger;

        public ExamGenerationService(ApplicationDbContext context, ILogger<ExamGenerationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task GenerateExamTimetableAsync(DateTime examStartDate)
        {
            try
            {
                // Clear existing exams (optional - you might want to keep historical data)
                _context.Exams.RemoveRange(_context.Exams);
                await _context.SaveChangesAsync();

                // Get all active course units with their relationships
                var courseUnits = await _context.CourseUnits
                    .Include(cu => cu.ProgrammeCourseUnits)
                        .ThenInclude(pcu => pcu.Programme)
                    .Include(cu => cu.SubjectCombinationCourseUnits)
                        .ThenInclude(scu => scu.SubjectCombination)
                    .Include(cu => cu.SubjectCombinationCourseUnits)
                        .ThenInclude(scu => scu.Year)
                    .Where(cu => !_context.Exams.Any(e => e.CourseUnitId == cu.CourseUnitId))
                    .ToListAsync();

                // Get all available rooms with invigilators
                var rooms = await _context.Rooms
                    .Include(r => r.Invigilators)
                    .OrderByDescending(r => r.Capacity)
                    .ToListAsync();

                // Group courses by year
                var coursesByYear = courseUnits
                    .GroupBy(cu => GetPrimaryYear(cu))
                    .OrderBy(g => g.Key) // Order by year (HEAC, 1, 2, 3)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Calculate working days (exclude weekends)
                var endDate = examStartDate.AddDays(20); // 15 working days
                var workingDays = GetWorkingDays(examStartDate, endDate).ToList();

                // Calculate exams per day per year
                var totalExams = courseUnits.Count;
                var examsPerDay = (int)Math.Ceiling((double)totalExams / workingDays.Count);
                var examsPerYearPerDay = (int)Math.Ceiling((double)examsPerDay / coursesByYear.Count);

                // Track scheduled exams and resource usage
                var scheduledExams = new List<Exam>();
                var bookedRooms = new Dictionary<DateTime, Dictionary<TimeSlot, List<Room>>>();
                var bookedInvigilators = new Dictionary<DateTime, List<Invigilator>>();

                // Initialize booking trackers for each day
                foreach (var day in workingDays)
                {
                    bookedRooms[day] = new Dictionary<TimeSlot, List<Room>>
                    {
                        [TimeSlot.Morning] = new List<Room>(),
                        [TimeSlot.Afternoon] = new List<Room>()
                    };
                    bookedInvigilators[day] = new List<Invigilator>();
                }

                // Schedule exams for each working day
                foreach (var day in workingDays)
                {
                    // Process morning and afternoon slots
                    foreach (var slot in new[] { TimeSlot.Morning, TimeSlot.Afternoon })
                    {
                        // Schedule exams for each year in this slot
                        foreach (var year in coursesByYear.Keys)
                        {
                            var coursesToSchedule = GetCoursesToSchedule(
                                coursesByYear[year],
                                examsPerYearPerDay,
                                scheduledExams);

                            foreach (var course in coursesToSchedule)
                            {
                                var exam = await ScheduleExam(
                                    course,
                                    day,
                                    slot,
                                    rooms,
                                    bookedRooms[day][slot],
                                    bookedInvigilators[day]);

                                if (exam != null)
                                {
                                    scheduledExams.Add(exam);
                                    _context.Exams.Add(exam);
                                    await _context.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating exam timetable");
                throw;
            }
        }

        private async Task<Exam> ScheduleExam(
            CourseUnit course,
            DateTime date,
            TimeSlot slot,
            List<Room> allRooms,
            List<Room> bookedRoomsForSlot,
            List<Invigilator> bookedInvigilatorsForDay)
        {
            // Calculate required capacity
            var requiredCapacity = course.EstimatedParticipants ?? 0;
            if (requiredCapacity <= 0) return null;

            // Find available rooms
            var availableRooms = allRooms
                .Except(bookedRoomsForSlot)
                .OrderByDescending(r => r.Capacity)
                .ToList();

            var assignedRooms = new List<Room>();
            var remainingCapacity = requiredCapacity;

            // Assign rooms based on capacity
            foreach (var room in availableRooms)
            {
                if (remainingCapacity <= 0) break;

                assignedRooms.Add(room);
                bookedRoomsForSlot.Add(room);
                remainingCapacity -= room.Capacity;
            }

            // If we couldn't find enough capacity, skip this course for now
            if (remainingCapacity > 0) return null;

            // Assign invigilators to each room
            var examRooms = new List<ExamRoom>();
            foreach (var room in assignedRooms)
            {
                var availableInvigilators = room.Invigilators?
                    .Where(i => !bookedInvigilatorsForDay.Contains(i))
                    .Take(room.Capacity > 100 ? 2 : 1)
                    .ToList();

                if (availableInvigilators == null || !availableInvigilators.Any())
                    return null; // No available invigilators

                // Create ExamRoom with invigilators
                var examRoom = new ExamRoom
                {
                    RoomId = room.RoomId,
                    Invigilators = availableInvigilators.Select(i => new ExamRoomInvigilator
                    {
                        InvigilatorId = i.InvigilatorId
                    }).ToList()
                };

                examRooms.Add(examRoom);
                bookedInvigilatorsForDay.AddRange(availableInvigilators);
            }

            // Create the exam
            var exam = new Exam
            {
                CourseUnitId = course.CourseUnitId,
                ExamDate = date,
                Duration = 3, // All exams are 3 hours
                StartTime = slot == TimeSlot.Morning
                    ? new TimeOnly(9, 0)
                    : new TimeOnly(14, 0),
                ExamRooms = examRooms
            };

            return exam;
        }

        private List<CourseUnit> GetCoursesToSchedule(
            List<CourseUnit> courses,
            int count,
            List<Exam> scheduledExams)
        {
            // Get courses not yet scheduled
            var unscheduledCourses = courses
                .Where(c => !scheduledExams.Any(e => e.CourseUnitId == c.CourseUnitId))
                .ToList();

            // Randomize selection but prioritize courses with more students
            return unscheduledCourses
                .OrderByDescending(c => c.EstimatedParticipants)
                .ThenBy(_ => Guid.NewGuid()) // Add some randomness
                .Take(count)
                .ToList();
        }

        private string GetPrimaryYear(CourseUnit course)
        {
            // Get the most common year from programme and subject combinations
            var years = new List<string>();

            if (course.ProgrammeCourseUnits?.Any() == true)
            {
                years.AddRange(course.ProgrammeCourseUnits
                    .Select(pcu => pcu.Year?.Name ?? "HEAC"));
            }

            if (course.SubjectCombinationCourseUnits?.Any() == true)
            {
                years.AddRange(course.SubjectCombinationCourseUnits
                    .Select(scu => scu.Year?.Name ?? "HEAC"));
            }

            return years.GroupBy(y => y)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "HEAC";
        }

        private IEnumerable<DateTime> GetWorkingDays(DateTime startDate, DateTime endDate)
        {
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    yield return date;
                }
            }
        }
    }

    public enum TimeSlot { Morning, Afternoon }
}