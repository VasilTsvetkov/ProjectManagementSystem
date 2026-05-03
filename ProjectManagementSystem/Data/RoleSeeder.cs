namespace ProjectManagementSystem.Data
{
    using Constants;
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
            var adminUser = await userManager.FindByEmailAsync(SeedData.AdminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = SeedData.AdminEmail,
                    Email = SeedData.AdminEmail,
                    FirstName = SeedData.AdminFirstName,
                    LastName = SeedData.AdminLastName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, SeedData.AdminPassword);

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