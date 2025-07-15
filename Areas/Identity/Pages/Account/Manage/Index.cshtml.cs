using ExamTimeTable.Data;
using ExamTimeTable.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ExamTimeTable.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _context = context;
            _env = env;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public IFormFile ProfileImage { get; set; }

        public string CurrentProfilePicture { get; set; }
        public string UserRole { get; set; }

        public SelectList Programmes { get; set; }
        public SelectList Years { get; set; }
        public SelectList Departments { get; set; }

        public class InputModel
        {
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Display(Name = "Email")]
            public string Email { get; set; }

            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            // Student
            [Display(Name = "Student Number")]
            public string StudentNumber { get; set; }

            public int? ProgrammeId { get; set; }
            public int? YearId { get; set; }

            // Invigilator
            public int? DepartmentId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.Users
                .Include(u => u.Programme).ThenInclude(p => p.Year)
                .Include(u => u.Department)
                .Include(u => u.Year)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user == null) return NotFound("User not found.");

            CurrentProfilePicture = user.ProfilePicture ?? "default.png";
            var roles = await _userManager.GetRolesAsync(user);
            UserRole = roles.FirstOrDefault() ?? "User";

            Input = new InputModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                StudentNumber = user.StudentNumber,
                ProgrammeId = user.ProgrammeId,
                YearId = user.YearId,
                DepartmentId = user.DepartmentId
            };

            Programmes = new SelectList(await _context.Programmes.Include(p => p.Year)
                .Select(p => new
                {
                    p.ProgrammeId,
                    Display = p.Name + (p.Year != null ? $" - {p.Year.Name}" : "")
                }).ToListAsync(), "ProgrammeId", "Display");

            Years = new SelectList(await _context.Years.ToListAsync(), "YearId", "Name");

            Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            if (user == null) return NotFound("User not found.");

            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            // Update text fields
            user.FullName = Input.FullName;
            user.PhoneNumber = Input.PhoneNumber;

            if (user.Email.EndsWith("@busitema.ac.ug") && !user.Email.Contains("@staff."))
            {
                user.StudentNumber = Input.StudentNumber;
                user.ProgrammeId = Input.ProgrammeId;
                user.YearId = Input.YearId;
            }
            else if (user.Email.EndsWith("@staff.busitema.ac.ug"))
            {
                user.DepartmentId = Input.DepartmentId;
            }

            // Handle profile image upload
            if (ProfileImage != null)
            {
                var extension = Path.GetExtension(ProfileImage.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(_env.WebRootPath, "images", "profiles", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(user.ProfilePicture) && user.ProfilePicture != "default.png")
                {
                    var oldPath = Path.Combine(_env.WebRootPath, "images", "profiles", user.ProfilePicture);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                user.ProfilePicture = fileName;
            }

            await _userManager.UpdateAsync(user);
            TempData["ToastMessage"] = "Account details updated successfully!";
            TempData["ToastType"] = "success";

            return RedirectToPage();
        }

        private async Task LoadSelectListsAsync()
        {
            Programmes = new SelectList(await _context.Programmes.Include(p => p.Year)
                .Select(p => new
                {
                    p.ProgrammeId,
                    Display = p.Name + (p.Year != null ? $" - {p.Year.Name}" : "")
                }).ToListAsync(), "ProgrammeId", "Display");

            Years = new SelectList(await _context.Years.ToListAsync(), "YearId", "Name");

            Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "Name");
        }
    }
}
