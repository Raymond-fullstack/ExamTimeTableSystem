using ExamTimeTable.Data;
using ExamTimeTable.Models;
using ExamTimeTable.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExamTimeTable.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InvigilatorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InvigilatorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Invigilator
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.DepartmentId == null)
                return Unauthorized();

            var invigilators = await _context.Invigilators
                .Where(i => i.DepartmentId == currentUser.DepartmentId)
                .Include(i => i.Department)
                .ToListAsync();

            return View(invigilators);
        }

        // GET: Invigilator/Assign
        public async Task<IActionResult> Assign()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.DepartmentId == null)
                return Unauthorized();

            var usersWithRole = await _userManager.GetUsersInRoleAsync("Invigilator");

            var availableUsers = usersWithRole
                .Where(u => u.DepartmentId == currentUser.DepartmentId && u.InvigilatorId == null)
                .ToList();

            var model = new InvigilatorAssignViewModel
            {
                AvailableInvigilators = availableUsers.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FullName ?? u.Email
                }).ToList()
            };

            return View(model); // <-- this is the key part
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(InvigilatorAssignViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var departmentId = currentUser?.DepartmentId;

            if (string.IsNullOrWhiteSpace(model.SelectedUserId) || departmentId == null)
            {
                ModelState.AddModelError("SelectedUserId", "Please select a valid invigilator.");
            }

            if (!ModelState.IsValid)
            {
                var usersWithRole = await _userManager.GetUsersInRoleAsync("Invigilator");
                var availableUsers = usersWithRole
                    .Where(u => u.DepartmentId == departmentId && u.InvigilatorId == null)
                    .ToList();

                model.AvailableInvigilators = availableUsers.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FullName ?? u.Email
                }).ToList();

                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == model.SelectedUserId &&
                                          u.DepartmentId == departmentId &&
                                          !_context.Invigilators.Any(i => i.InvigilatorId == u.InvigilatorId));

            if (user == null)
            {
                TempData["Error"] = "Invalid user selection or user already assigned.";
                return RedirectToAction(nameof(Assign));
            }

            var invigilator = new Invigilator
            {
                Name = user.FullName,
                PhoneNumber = user.PhoneNumber,
                DepartmentId = departmentId.Value,
            };

            _context.Invigilators.Add(invigilator);
            await _context.SaveChangesAsync();

            user.InvigilatorId = invigilator.InvigilatorId;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Invigilator assigned successfully.";
            return RedirectToAction(nameof(Index));
        }



        // Optional: Remove assigned Invigilator
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var invigilator = await _context.Invigilators.FindAsync(id);
            if (invigilator == null)
                return NotFound();

            var linkedUser = await _context.Users.FirstOrDefaultAsync(u => u.InvigilatorId == id);
            if (linkedUser != null)
            {
                linkedUser.InvigilatorId = null;
                await _userManager.UpdateAsync(linkedUser);
            }

            _context.Invigilators.Remove(invigilator);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
