namespace ProjectManagementSystem.Core.Interfaces
{
    using Common.DTOs.Dashboard;

    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync(int? year, int? month, string userId);
    }
}