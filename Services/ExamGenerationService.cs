// Services/ExamGenerationService.cs
using ExamTimeTable.Data;
using ExamTimeTable.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly Random _random; // Added Random instance

        public ExamGenerationService(ApplicationDbContext context, ILogger<ExamGenerationService> logger)
        {
            _context = context;
            _logger = logger;
            _random = new Random(); // Initialize Random once per service instance
        }

        public async Task GenerateExamTimetableAsync(DateTime examStartDate)
        {
            try
            {
                _logger.LogInformation("Starting exam timetable generation for {StartDate}", examStartDate);

                // Clear existing exams (optional - you might want to keep historical data)
                // This will also cascade delete ExamRooms and ExamRoomInvigilators if configured properly.
                _context.Exams.RemoveRange(_context.Exams);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Existing exams cleared.");

                // Get all active course units with their relationships
                var courseUnits = await _context.CourseUnits
                    .Include(cu => cu.ProgrammeCourseUnits)
                        .ThenInclude(pcu => pcu.Programme) // Ensure Programme is included for EstimatedParticipants
                        .ThenInclude(pcu => pcu.Year)      // Ensure Year is included for GetPrimaryYear
                    .Include(cu => cu.SubjectCombinationCourseUnits)
                        .ThenInclude(scu => scu.SubjectCombination) // Ensure SubjectCombination is included for EstimatedParticipants
                        .ThenInclude(scu => scu.Programme.Year)      // Ensure Year is included for GetPrimaryYear

                    .ToListAsync();

                if (!courseUnits.Any())
                {
                    _logger.LogWarning("No course units found to schedule exams for.");
                    return;
                }

                // Get all available rooms with their assigned invigilators (if invigilators are permanently tied to rooms)
                var rooms = await _context.Rooms
                    .Include(r => r.Invigilators) // This implies Room has a collection of Invigilators
                    .OrderByDescending(r => r.Capacity)
                    .ToListAsync();

                if (!rooms.Any())
                {
                    _logger.LogError("No rooms available for scheduling exams.");
                    throw new InvalidOperationException("No rooms available for scheduling exams.");
                }

                // Group courses by year (HEAC, year1, year2, year3)
                var coursesByYear = courseUnits
                    .GroupBy(cu => GetPrimaryYear(cu))
                    .OrderBy(g => g.Key) // Order by year (HEAC, 1, 2, 3)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Calculate working days (exclude weekends), max 15 days
                var workingDays = new List<DateTime>();
                DateTime currentDay = examStartDate.Date; // Start from the exact date
                int daysCounted = 0;

                while (daysCounted < 15 && currentDay < examStartDate.AddMonths(1)) // Safety break: max 1 month search
                {
                    if (currentDay.DayOfWeek != DayOfWeek.Saturday && currentDay.DayOfWeek != DayOfWeek.Sunday)
                    {
                        workingDays.Add(currentDay);
                        daysCounted++;
                    }
                    currentDay = currentDay.AddDays(1);
                }

                if (!workingDays.Any())
                {
                    _logger.LogError("Could not determine valid working days for exam timetable within the 3-week period.");
                    throw new InvalidOperationException("Could not determine valid working days for exam timetable.");
                }

                // Calculate exams per day per year
                var totalExams = courseUnits.Count;
                var examsPerDay = (int)Math.Ceiling((double)totalExams / workingDays.Count);
                var examsPerYearPerDay = coursesByYear.Count > 0
                    ? (int)Math.Ceiling((double)examsPerDay / coursesByYear.Count)
                    : 1;
                examsPerYearPerDay = Math.Max(1, examsPerYearPerDay); // Ensure at least 1 exam per year per day if possible

                // Track scheduled exams and resource usage
                var scheduledExams = new List<Exam>();
                // Booked Rooms: Key: Date, Value: Dictionary (Key: TimeSlot, Value: HashSet of RoomIds booked for that slot)
                var bookedRooms = new Dictionary<DateTime, Dictionary<TimeSlot, HashSet<int>>>();
                // Booked Invigilators: Key: Date, Value: HashSet of InvigilatorIds booked for that day
                var bookedInvigilators = new Dictionary<DateTime, HashSet<int>>();

                // Initialize booking trackers for each day
                foreach (var day in workingDays)
                {
                    bookedRooms[day] = new Dictionary<TimeSlot, HashSet<int>>
                    {
                        [TimeSlot.Morning] = new HashSet<int>(),
                        [TimeSlot.Afternoon] = new HashSet<int>()
                    };
                    bookedInvigilators[day] = new HashSet<int>();
                }

                // Create a mutable copy of course units grouped by year, randomized for fair selection
                var coursesToProcessByYear = coursesByYear.ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value.OrderBy(_ => _random.Next()).ToList() // Randomize initially
                );


                // Schedule exams for each working day
                foreach (var day in workingDays)
                {
                    _logger.LogInformation("Scheduling for {Day}", day.ToShortDateString());

                    // Process morning and afternoon slots
                    foreach (var slot in new[] { TimeSlot.Morning, TimeSlot.Afternoon })
                    {
                        _logger.LogInformation("  Shift: {Slot}", slot);

                        // Shuffle years to distribute exams more evenly across them in this slot
                        var shuffledYears = coursesByYear.Keys.OrderBy(_ => _random.Next()).ToList();

                        foreach (var year in shuffledYears)
                        {
                            if (!coursesToProcessByYear.TryGetValue(year, out var coursesInThisYear) || !coursesInThisYear.Any())
                            {
                                continue; // No courses left for this year
                            }

                            // Determine how many exams to attempt for this year in this slot
                            int examsToAttemptForThisYearThisSlot = examsPerYearPerDay;
                            _logger.LogDebug("    Year: {Year}, Aiming for {Count} exams.", year, examsToAttemptForThisYearThisSlot);

                            for (int i = 0; i < examsToAttemptForThisYearThisSlot; i++)
                            {
                                // Find an eligible CourseUnit that hasn't been scheduled yet
                                var eligibleCourse = coursesInThisYear
                                    .FirstOrDefault(c => !scheduledExams.Any(e => e.CourseUnitId == c.CourseUnitId));

                                if (eligibleCourse == null)
                                {
                                    _logger.LogDebug("      No more eligible CourseUnits for year {Year}.", year);
                                    break; // No more eligible courses for this year
                                }

                                var exam = await ScheduleExam(
                                    eligibleCourse,
                                    day,
                                    slot,
                                    rooms, // All available rooms
                                    bookedRooms[day][slot], // Rooms booked for current slot on current day
                                    bookedInvigilators[day]); // Invigilators booked for current day

                                if (exam != null)
                                {
                                    scheduledExams.Add(exam);
                                    // Remove the course unit from the list to avoid re-scheduling
                                    coursesToProcessByYear[year].Remove(eligibleCourse);
                                    _logger.LogDebug("      Scheduled CourseUnit {CUCode} for {Slot}.", eligibleCourse.Code, slot);
                                }
                                else
                                {
                                    _logger.LogWarning("      Failed to schedule CourseUnit {CUCode} for {Slot} due to resource constraints.", eligibleCourse.Code, slot);
                                    // If an exam couldn't be scheduled due to resource constraints,
                                    // we skip it for this slot/day. It will be attempted on subsequent days/slots.
                                }
                            }
                        }
                    }
                }

                // Add all generated exams to the context for bulk saving
                foreach (var exam in scheduledExams)
                {
                    _context.Exams.Add(exam);
                    // EF Core's change tracker handles nested entities if correctly configured,
                    // so ExamRooms and ExamRoomInvigilators will be added along with Exam.
                }

                // Persist all changes to the database at once
                await _context.SaveChangesAsync();
                _logger.LogInformation("Exam timetable generation completed. {Count} exams scheduled.", scheduledExams.Count);

                // Identify any remaining unscheduled course units
                var unscheduledCount = courseUnits.Count(cu => !scheduledExams.Any(e => e.CourseUnitId == cu.CourseUnitId));
                if (unscheduledCount > 0)
                {
                    _logger.LogWarning("{UnscheduledCount} course units could not be scheduled.", unscheduledCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating exam timetable");
                throw; // Re-throw to be handled by the controller
            }
        }

        private async Task<Exam> ScheduleExam(
            CourseUnit course,
            DateTime date,
            TimeSlot slot,
            List<Room> allRooms, // Use the full list of rooms
            HashSet<int> bookedRoomsForSlot, // Use HashSet for faster lookups of booked rooms
            HashSet<int> bookedInvigilatorsForDay) // Use HashSet for faster lookups of booked invigilators
        {
            var requiredCapacity = course.EstimatedParticipants ?? 0;
            if (requiredCapacity <= 0)
            {
                _logger.LogDebug("CourseUnit {CUCode} has no estimated participants or 0. Skipping.", course.Code);
                return null;
            }

            var assignedRooms = new List<Room>();
            var remainingCapacity = requiredCapacity;

            // Prioritize larger rooms first for better packing, then by randomness to break ties
            var availableRoomsForSelection = allRooms
                .Where(r => !bookedRoomsForSlot.Contains(r.RoomId))
                .OrderByDescending(r => r.Capacity)
                .ThenBy(_ => _random.Next()) // Add some randomness for selection
                .ToList();

            foreach (var room in availableRoomsForSelection)
            {
                if (remainingCapacity <= 0) break; // Enough capacity found

                assignedRooms.Add(room);
                bookedRoomsForSlot.Add(room.RoomId); // Mark room ID as booked
                remainingCapacity -= room.Capacity;
            }

            if (remainingCapacity > 0)
            {
                _logger.LogWarning("Not enough room capacity for CourseUnit {CUCode} (needed {Needed}, got {Got}).", course.Code, requiredCapacity, requiredCapacity - remainingCapacity);
                // Revert booked rooms if the exam cannot be fully accommodated
                foreach (var room in assignedRooms)
                {
                    bookedRoomsForSlot.Remove(room.RoomId);
                }
                return null; // Not enough capacity, cannot schedule this exam
            }

            // Create the Exam entity
            var exam = new Exam
            {
                CourseUnitId = course.CourseUnitId,
                ExamDate = date,
                Duration = 3, // All exams are 3 hours
                StartTime = slot == TimeSlot.Morning
                    ? new TimeOnly(9, 0)
                    : new TimeOnly(14, 0),
                ExamRooms = new List<ExamRoom>()
            };

            // Assign rooms and invigilators for each assigned room
            foreach (var room in assignedRooms)
            {
                var examRoom = new ExamRoom
                {
                    RoomId = room.RoomId,
                    Invigilators = new List<ExamRoomInvigilator>() // Initialize the collection
                };

                // Determine invigilators needed for this room
                int invigilatorsNeeded = room.Capacity > 100 ? 2 : 1;

                // Find invigilators assigned to *this specific room* in the database (via room.Invigilators)
                // and are not yet booked for the day.
                var eligibleInvigilatorsForRoom = room.Invigilators?
                    .Where(i => !bookedInvigilatorsForDay.Contains(i.InvigilatorId))
                    .OrderBy(_ => _random.Next()) // Randomize selection from eligible pool
                    .ToList() ?? new List<Invigilator>(); // Ensure it's not null

                for (int invCount = 0; invCount < invigilatorsNeeded; invCount++)
                {
                    if (eligibleInvigilatorsForRoom.Any())
                    {
                        Invigilator invigilatorToAssign = eligibleInvigilatorsForRoom.First(); // Take the first random one
                        eligibleInvigilatorsForRoom.RemoveAt(0); // Remove from list to avoid double-assignment for this room/exam

                        examRoom.Invigilators.Add(new ExamRoomInvigilator
                        {
                            InvigilatorId = invigilatorToAssign.InvigilatorId,
                            RoomId = room.RoomId, // Explicitly set RoomId in ExamRoomInvigilator for clarity and direct linkage
                            ExamId = exam.ExamId // Explicitly set ExamId too
                        });
                        bookedInvigilatorsForDay.Add(invigilatorToAssign.InvigilatorId); // Mark invigilator as booked for the day
                    }
                    else
                    {
                        _logger.LogWarning("Not enough invigilators available for room {RoomCode} for CourseUnit {CUCode}. Required {Needed}, Found {Found}.", room.Code, course.Code, invigilatorsNeeded, invCount);
                        // This exam might be scheduled with fewer invigilators than ideal, or you could return null here to fail the scheduling.
                        // For now, it proceeds with fewer invigilators if necessary.
                        break; // Stop trying to assign more invigilators to this room
                    }
                }
                exam.ExamRooms.Add(examRoom);
            }

            return exam;
        }

        // Note: GetCoursesToSchedule is primarily for prioritizing selection.
        // The main loop handles ensuring courses are not double-scheduled.
        private List<CourseUnit> GetCoursesToSchedule(
            List<CourseUnit> courses,
            int count,
            List<Exam> scheduledExams)
        {
            // Get courses not yet scheduled
            var unscheduledCourses = courses
                .Where(c => !scheduledExams.Any(e => e.CourseUnitId == c.CourseUnitId))
                .ToList();

            // Prioritize courses with more students first, then randomize remaining
            return unscheduledCourses
                .OrderByDescending(c => c.EstimatedParticipants)
                .ThenBy(_ => _random.Next())
                .Take(count)
                .ToList();
        }

        private string GetPrimaryYear(CourseUnit course)
        {
            var years = new List<string>();

            if (course.ProgrammeCourseUnits?.Any() == true)
            {
                years.AddRange(course.ProgrammeCourseUnits
                    .Select(pcu => pcu.Year?.Name)
                    .Where(name => !string.IsNullOrEmpty(name)));
            }

            if (course.SubjectCombinationCourseUnits?.Any() == true)
            {
                years.AddRange(course.SubjectCombinationCourseUnits
                    .Select(scu => scu.Year?.Name)
                    .Where(name => !string.IsNullOrEmpty(name)));
            }

            return years.GroupBy(y => y)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "HEAC";
        }
    }

    public enum TimeSlot { Morning, Afternoon }
}