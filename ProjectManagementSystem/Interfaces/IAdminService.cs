namespace ProjectManagementSystem.Interfaces
{
    using ViewModels.Admin;

    public interface IAdminService
    {
        Task<IEnumerable<UserRoleViewModel>> GetAllUsersWithRolesAsync();
        Task<(bool Success, string Message)> ChangeUserRoleAsync(string userId, string newRole);
    }
}