namespace ProjectManagementSystem.Data
{
    using Enums;
    using Helpers;
    using Microsoft.AspNetCore.Identity;
    using Models;

    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            foreach (var roleName in RoleHelper.GetAllRoleNames())
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            await SeedDefaultAdminAsync(userManager);
        }

        private static async Task SeedDefaultAdminAsync(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@pms.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToRoleName());
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, UserRole.Admin.ToRoleName()))
                {
                    await userManager.AddToRoleAsync(adminUser, UserRole.Admin.ToRoleName());
                }
            }
        }
    }
}