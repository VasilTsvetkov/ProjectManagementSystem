namespace ProjectManagementSystem.Data
{
    using Constants;
    using Microsoft.AspNetCore.Identity;
    using Models;

    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            foreach (var roleName in Roles.All)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            await SeedDefaultAdminAsync(userManager, configuration);
        }

        private static async Task SeedDefaultAdminAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            var adminEmail = configuration["AdminSettings:Email"];
            var adminPassword = configuration["AdminSettings:Password"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                throw new InvalidOperationException("Critical Startup Error: Admin credentials are missing in the configuration (secrets.json). The system cannot seed the default administrator.");
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = SeedData.AdminFirstName,
                    LastName = SeedData.AdminLastName,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, Roles.Admin))
                {
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                }
            }
        }
    }
}