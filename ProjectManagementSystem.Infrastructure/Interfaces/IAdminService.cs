namespace ProjectManagementSystem.BL.Interfaces
{
    using DTOs;

    public interface IAdminService
    {
        Task<IEnumerable<UserRoleDto>> GetAllUsersWithRolesAsync();

        Task<(bool Success, string Message)> ChangeUserRoleAsync(string userId, string newRole);
    }
}