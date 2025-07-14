// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using ExamTimeTable.Data;
using ExamTimeTable.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace ExamTimeTable.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            // Student-specific
            [Display(Name = "Student Number")]
            public string? StudentNumber { get; set; }

            [Display(Name = "Program")]
            public int? ProgrammeId { get; set; }

            [Display(Name = "Year")]
            public int? YearId { get; set; }

            // Invigilator-specific
            [Display(Name = "Department")]
            public int? DepartmentId { get; set; }

            [Required]
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "Passwords don't match.")]
            public string ConfirmPassword { get; set; }
        }

        public SelectList Programmes { get; set; }
        public SelectList Years { get; set; }
        public SelectList Departments { get; set; }
        public SelectList SubjectCombinations { get; set; }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            Programmes = new SelectList(await _context.Programmes
            .Include(sc => sc.SubjectCombinations)
            .Include(p => p.Year)
            .Select(p => new
            {
                p.ProgrammeId,
                Display = p.Name + " - " + (p.Year != null ? p.Year.Name : "")
            }).ToListAsync(), "ProgrammeId", "Display");

            Years = new SelectList(await _context.Years.ToListAsync(), "YearId", "Name");
            Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "Name");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FullName = Input.FullName,
                    PhoneNumber = Input.PhoneNumber,
                    ProfilePicture = "default.png"
                };

                // Determine user type based on email
                if (Input.Email.EndsWith("@busitema.ac.ug") && !Input.Email.Contains("@staff."))
                {
                    // Student
                    user.StudentNumber = Input.StudentNumber;
                    user.ProgrammeId = Input.ProgrammeId;
                    user.YearId = Input.YearId;
                }
                else if (Input.Email.EndsWith("@staff.busitema.ac.ug"))
                {
                    // Invigilator
                    user.DepartmentId = Input.DepartmentId;
                }

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    // Assign Role
                    if (Input.Email.EndsWith("@busitema.ac.ug") && !Input.Email.Contains("@staff."))
                    {
                        await _userManager.AddToRoleAsync(user, "Student");
                    }
                    else if (Input.Email.EndsWith("@staff.busitema.ac.ug"))
                    {
                        await _userManager.AddToRoleAsync(user, "Invigilator");
                    }

                    _logger.LogInformation("User created a new account with password.");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed — reload dropdowns
            Programmes = new SelectList(await _context.Programmes.Include(p => p.Year)
                .Select(p => new
                {
                    p.ProgrammeId,
                    Display = p.Name + " - " + (p.Year != null ? p.Year.Name : "")
                }).ToListAsync(), "ProgrammeId", "Display");

            Years = new SelectList(await _context.Years.ToListAsync(), "YearId", "Name");
            Departments = new SelectList(await _context.Departments.ToListAsync(), "DepartmentId", "Name");

            return Page();
        }


        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
