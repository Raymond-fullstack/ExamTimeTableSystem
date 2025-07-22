using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExamTimeTable.Data; // Assuming this is your DbContext namespace
using ExamTimeTable.Models; // Assuming this is where ApplicationUser, Department, Programme, CourseUnit, Room, Invigilator, Exam models are located
using ExamTimeTable.ViewModels; // Assuming this is where your AdminDashboardViewModel is located
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Added for List

namespace ExamTimeTable.Controllers
{
    // You might want to add authorization here, e.g., [Authorize(Roles = "SystemAdmin,Admin,Invigilator")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch total counts for summary cards
            var totalDepartments = await _context.Departments.CountAsync();
            var totalProgrammes = await _context.Programmes.CountAsync();
            var totalCombinations = await _context.SubjectCombinations.CountAsync();
            var totalCourseUnits = await _context.CourseUnits.CountAsync();
            var totalRooms = await _context.Rooms.CountAsync();
            var totalInvigilators = await _context.Invigilators.CountAsync();

            // Calculate Upcoming Exams
            var upcomingExams = await _context.Exams
                                            .Include(e => e.CourseUnit) // Eager load CourseUnit
                                            //.Include(e => e.Room)         // Eager load Room
                                            //.Include(e => e.Invigilator)  // Eager load Invigilator
                                            .Where(e => e.ExamDate.Date >= DateTime.Today)
                                            .OrderBy(e => e.ExamDate)
                                            .ThenBy(e => e.StartTime)
                                            .Take(5) // Limit to a reasonable number for dashboard display
                                            .ToListAsync();

            // Logic to count users with ONLY the "User" role
            var usersInUserRole = await _userManager.GetUsersInRoleAsync("User");

            // Define all other roles that a user *could* have.
            // Adjust this list based on the actual roles defined in your application.
            var allOtherRoles = new List<string> { "SystemAdmin", "Admin", "Invigilator" };

            int totalRegularUsers = 0;
            foreach (var user in usersInUserRole)
            {
                bool hasOtherRoles = false;
                foreach (var role in allOtherRoles)
                {
                    if (await _userManager.IsInRoleAsync(user, role))
                    {
                        hasOtherRoles = true;
                        break; // This user has another role, so they are not *only* a "User"
                    }
                }

                if (!hasOtherRoles)
                {
                    totalRegularUsers++;
                }
            }


            var model = new AdminDashboardViewModel
            {
                TotalDepartments = totalDepartments,
                TotalProgrammes = totalProgrammes,
                TotalCombinations = totalCombinations,
                TotalCourseUnits = totalCourseUnits,
                TotalRooms = totalRooms,
                TotalInvigilators = totalInvigilators,
                UpcomingExams = upcomingExams,
                TotalUsers = totalRegularUsers // This now correctly represents users with ONLY the "User" role
            };

            return View(model);
        }
    }
}