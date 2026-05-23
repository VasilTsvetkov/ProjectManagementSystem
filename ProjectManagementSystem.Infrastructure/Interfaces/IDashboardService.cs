namespace ProjectManagementSystem.BL.Interfaces
{
    using DTOs.Dashboard;

    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync(int? year, int? month, string userId);
    }
}