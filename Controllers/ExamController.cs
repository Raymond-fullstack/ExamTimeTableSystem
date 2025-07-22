using ExamTimeTable.Data;
using ExamTimeTable.Models;
using ExamTimeTable.Services;
using ExamTimeTable.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Ensure this is present for .FirstOrDefault() on collections

namespace ExamTimeTable.Controllers
{
    public class ExamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IExamGenerationService _examGenerationService;
        private readonly ILogger<ExamController> _logger;

        public ExamController(
            ApplicationDbContext context,
            IExamGenerationService examGenerationService,
            ILogger<ExamController> logger)
        {
            _context = context;
            _examGenerationService = examGenerationService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var exams = await _context.Exams
                .Include(e => e.CourseUnit)
                    .ThenInclude(cu => cu.ProgrammeCourseUnits) // Load ProgrammeCourseUnits
                        .ThenInclude(pcu => pcu.Programme) // Load related Programme
                .Include(e => e.CourseUnit)
                    .ThenInclude(cu => cu.ProgrammeCourseUnits) // Load ProgrammeCourseUnits again for Year
                        .ThenInclude(pcu => pcu.Year) // Load related Year
                .Include(e => e.ExamRooms)
                    .ThenInclude(er => er.Room)
                .Include(e => e.ExamRooms)
                    .ThenInclude(er => er.Invigilators)
                        .ThenInclude(ei => ei.Invigilator)
                .OrderBy(e => e.ExamDate)
                .ThenBy(e => e.StartTime)
                .ToListAsync();

            // Fetch Programmes and Years separately for the filter dropdowns
            ViewBag.Programmes = await _context.Programmes.OrderBy(p => p.Name).ToListAsync();
            ViewBag.Years = await _context.Years.OrderBy(y => y.Name).ToListAsync();

            return View(exams);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> GenerateTimetable(DateTime startDate)
        {
            try
            {
                await _examGenerationService.GenerateExamTimetableAsync(startDate);
                TempData["ToastMessage"] = "Exam timetable generated successfully!";
                TempData["ToastType"] = "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating exam timetable");
                TempData["ToastMessage"] = "Error generating timetable: " + ex.Message;
                TempData["ToastType"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var exam = await _context.Exams
                .Include(e => e.CourseUnit)
                .Include(e => e.ExamRooms)
                    .ThenInclude(er => er.Room)
                .Include(e => e.ExamRooms)
                    .ThenInclude(er => er.Invigilators)
                    .ThenInclude(ei => ei.Invigilator)
                .FirstOrDefaultAsync(m => m.ExamId == id);

            if (exam == null) return NotFound();

            return View(exam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin, Admin")]
        public async Task<IActionResult> Reschedule(int examId, DateTime newDate, string timeSlot)
        {
            var exam = await _context.Exams
                .Include(e => e.ExamRooms)
                .FirstOrDefaultAsync(e => e.ExamId == examId);

            if (exam == null)
            {
                return NotFound();
            }

            // Validate new date is a weekday
            if (newDate.DayOfWeek == DayOfWeek.Saturday || newDate.DayOfWeek == DayOfWeek.Sunday)
            {
                TempData["ToastMessage"] = "Exams cannot be scheduled on weekends";
                TempData["ToastType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            // Update exam
            exam.ExamDate = newDate;
            exam.StartTime = timeSlot == "Morning"
                ? new TimeOnly(9, 0)
                : new TimeOnly(14, 0);

            try
            {
                _context.Update(exam);
                await _context.SaveChangesAsync();
                TempData["ToastMessage"] = "Exam rescheduled successfully!";
                TempData["ToastType"] = "success";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExamExists(exam.ExamId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            // IMPORTANT: This basic reschedule does NOT perform conflict checking (e.g., if the new slot is already taken by another exam for the same students/rooms/invigilators).
            // For robust rescheduling with conflict resolution, consider using the 'Generate New' timetable feature,
            // or implementing a more complex conflict-aware rescheduling logic in a service.

            return RedirectToAction(nameof(Index));
        }

        private bool ExamExists(int id) =>
            _context.Exams.Any(e => e.ExamId == id);
    }
}