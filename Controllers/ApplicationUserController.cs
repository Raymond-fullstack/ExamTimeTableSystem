using ExamTimeTable.Models;
using ExamTimeTable.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "SystemAdmin")]
public class ApplicationUserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationUserController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string search, int adminPage = 1, int studentPage = 1)
    {
        int pageSize = 10;

        var allUsers = await _userManager.Users
            .Include(u => u.Department)
            .Include(u => u.Programme)
            .Include(u => u.Year)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            allUsers = allUsers.Where(u =>
                u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var allWithRoles = new List<UserDisplayViewModel>();

        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            allWithRoles.Add(new UserDisplayViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = role,
                Department = user.Department?.Name,
                Programme = user.Programme?.Name,
                Year = user.Year?.Name
            });
        }


        var admins = allWithRoles.Where(u => u.Role == "Admin" || u.Role == "Invigilator").ToList();
        var students = allWithRoles.Where(u => u.Role == "User").ToList();

        return View(new UserListViewModel
        {
            SearchQuery = search ?? "",

            AdminsAndInvigilators = admins.Skip((adminPage - 1) * pageSize).Take(pageSize).ToList(),
            AdminCurrentPage = adminPage,
            AdminTotalPages = (int)Math.Ceiling((double)admins.Count / pageSize),

            Students = students.Skip((studentPage - 1) * pageSize).Take(pageSize).ToList(),
            StudentCurrentPage = studentPage,
            StudentTotalPages = (int)Math.Ceiling((double)students.Count / pageSize)
        });
    }




    [HttpPost]
    [Authorize(Roles = "SystemAdmin,Admin")]
    public async Task<IActionResult> ChangeRole(string userId, string newRole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, newRole);

        TempData["ToastMessage"] = "Role updated successfully!";
        TempData["ToastType"] = "info";

        return RedirectToAction("Index");
    }
}
