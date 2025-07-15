using ExamTimeTable.Models;
using Microsoft.AspNetCore.Identity;

namespace ExamTimeTable.Helpers
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "SystemAdmin", "Admin", "Invigilator", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Optional: Create default system admin
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var adminEmail = "superadmin@busitema.ac.ug";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "System Administrator"
                };

                var result = await userManager.CreateAsync(user, "Admin@123"); // Set strong password
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "SystemAdmin");
                }
            }
        }
    }
}
