namespace ProjectManagementSystem.Services
{
    using Enums;
    using Helpers;
    using Interfaces;
    using Microsoft.AspNetCore.Identity;
    using Models;
    using ViewModels.Admin;

    public class AdminService : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminService> _logger;

        public AdminService(UserManager<ApplicationUser> userManager, ILogger<AdminService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IEnumerable<UserRoleViewModel>> GetAllUsersWithRolesAsync()
        {
            var users = _userManager.Users.ToList();
            var userViewModels = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var currentRole = roles.FirstOrDefault() ?? UserRole.Member.ToRoleName();

                userViewModels.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? "No Email Provided",
                    FullName = UserDisplayNameHelper.GetFullName(user),
                    CurrentRole = currentRole,
                    IsAdmin = currentRole == UserRole.Admin.ToRoleName()
                });
            }

            return userViewModels;
        }

        public async Task<(bool Success, string Message)> ChangeUserRoleAsync(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Role change failed: User {UserId} not found", userId);
                return (false, "User not found");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var currentRole = currentRoles.FirstOrDefault();

            if (currentRole == UserRole.Admin.ToRoleName())
            {
                _logger.LogWarning("Security Alert: Unauthorized attempt to change Admin role for {Email}", user.Email);
                return (false, "Cannot change Admin role.");
            }

            if (currentRole != null)
            {
                await _userManager.RemoveFromRoleAsync(user, currentRole);
            }

            var result = await _userManager.AddToRoleAsync(user, newRole);

            if (result.Succeeded)
            {
                _logger.LogInformation("Role changed: {Email} updated from {OldRole} to {NewRole}", user.Email, currentRole, newRole);
                return (true, $"Role changed to {newRole} for {user.Email}");
            }

            return (false, "Failed to update role");
        }
    }
}