namespace ProjectManagementSystem.Services
{
    using Constants;
    using Interfaces;
    using Microsoft.AspNetCore.Identity;
    using Models;
    using ViewModels.Admin;
    using Microsoft.Extensions.Logging;

    public class AdminService(UserManager<ApplicationUser> userManager, ILogger<AdminService> logger) : IAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ILogger<AdminService> _logger = logger;

        public async Task<IEnumerable<UserRoleViewModel>> GetAllUsersWithRolesAsync()
        {
            var users = _userManager.Users.ToList();
            var userViewModels = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var currentRole = roles.FirstOrDefault() ?? Roles.Member;

                userViewModels.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? MessageConstants.NoEmailProvided,
                    FullName = user.FullName,
                    CurrentRole = currentRole,
                    IsAdmin = currentRole == Roles.Admin
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
                return (false, MessageConstants.UserNotFound);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var currentRole = currentRoles.FirstOrDefault();

            if (currentRole == Roles.Admin)
            {
                _logger.LogWarning("Security Alert: Unauthorized attempt to change Admin role for {Email}", user.Email);
                return (false, MessageConstants.CannotChangeAdminRole);
            }

            if (currentRole != null)
            {
                await _userManager.RemoveFromRoleAsync(user, currentRole);
            }

            var result = await _userManager.AddToRoleAsync(user, newRole);

            if (result.Succeeded)
            {
                _logger.LogInformation("Role changed: {FullName} updated from {OldRole} to {NewRole}", user.FullName, currentRole, newRole);
                return (true, string.Format(MessageConstants.RoleChangedSuccessfully, newRole, user.FullName));
            }

            return (false, MessageConstants.RoleUpdateFailed);
        }
    }
}