namespace ProjectManagementSystem.Interfaces
{
    using ViewModels.Dashboard;

    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(int? year, int? month, string userId);
    }
}