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
            try
            {
                _logger.LogInformation("Fetching all users with roles");

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

                _logger.LogInformation("Fetched {Count} users", userViewModels.Count);
                return userViewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users with roles");
                throw;
            }
        }

        public async Task<(bool Success, string Message)> ChangeUserRoleAsync(string userId, string newRole)
        {
            try
            {
                _logger.LogInformation("Changing role for user {UserId} to {NewRole}", userId, newRole);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return (false, "User not found");
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                var currentRole = currentRoles.FirstOrDefault();

                if (currentRole == UserRole.Admin.ToRoleName())
                {
                    _logger.LogWarning("Attempted to change Admin role for user {UserId}", userId);
                    return (false, "Cannot change Admin role.");
                }

                if (currentRole != null)
                {
                    await _userManager.RemoveFromRoleAsync(user, currentRole);
                }

                await _userManager.AddToRoleAsync(user, newRole);

                _logger.LogInformation("Successfully changed role to {NewRole} for user {Email}", newRole, user.Email);
                return (true, $"Role changed to {newRole} for {user.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing role for user {UserId}", userId);
                throw;
            }
        }
    }
}