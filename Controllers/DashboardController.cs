using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExamTimeTable.Data;
using ExamTimeTable.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace ExamTimeTable.Controllers
{
    [Authorize(Roles = "SystemAdmin, Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalDepartments = await _context.Departments.CountAsync(),
                TotalProgrammes = await _context.Programmes.CountAsync(),
                TotalCourseUnits = await _context.CourseUnits.CountAsync(),
                UpcomingExams = await _context.Exams
                    .Where(e => e.ExamDate >= DateTime.Today)
                    .OrderBy(e => e.ExamDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }
    }
}
