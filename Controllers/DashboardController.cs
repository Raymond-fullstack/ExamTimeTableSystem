using ExamTimeTable.Data;
using ExamTimeTable.Models;
using ExamTimeTable.Services;
using ExamTimeTable.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExamTimeTable.Controllers
{
    [Authorize(Roles = "SystemAdmin, Admin, Invigilator")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IExamGenerationService _examGenerationService;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IExamGenerationService examGenerationService)
        {
            _context = context;
            _userManager = userManager;
            _examGenerationService = examGenerationService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalDepartments = await _context.Departments.CountAsync(),
                TotalProgrammes = await _context.Programmes.CountAsync(),
                TotalCourseUnits = await _context.CourseUnits.CountAsync(),
                TotalRooms = await _context.Rooms.CountAsync(),
                TotalInvigilators = await _context.Invigilators.CountAsync(),
                UpcomingExams = await _context.Exams
                    .Include(e => e.CourseUnit)
                    .Where(e => e.ExamDate >= DateTime.Today)
                    .OrderBy(e => e.ExamDate)
                    .Take(10)
                    .ToListAsync()
            };

            return View(model);
        }

        //// GET: /Dashboard/GenerateTimetable
        //public IActionResult GenerateTimetable()
        //{
        //    var model = new ExamGenerationViewModel
        //    {
        //        ExamStartDate = DateTime.Today.AddDays(7) // default to a week from today
        //    };
        //    return View(model);
        //}

        //// POST: /Dashboard/GenerateTimetable
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> GenerateTimetable(ExamGenerationViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);

        //    }

        //    await _examGenerationService.GenerateExamTimetableAsync(model.ExamStartDate);
        //    TempData["ToastMessage"] = "Exam timetable generated successfully!";
        //    TempData["ToastType"] = "success";
        //    return RedirectToAction("Index");
        //}
    }
}
