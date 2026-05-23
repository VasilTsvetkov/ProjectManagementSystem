namespace ProjectManagementSystem.BL.Services
{
    using DTOs.Dashboard;
    using Interfaces;

    public class DashboardService(ITimeLogRepository timeLogRepository) : IDashboardService
    {
        private readonly ITimeLogRepository _timeLogRepository = timeLogRepository;

        public async Task<DashboardDto> GetDashboardDataAsync(int? year, int? month, string userId)
        {
            var selectedYear = year ?? DateTime.UtcNow.Year;
            var selectedMonth = month ?? DateTime.UtcNow.Month;

            var stats = await _timeLogRepository.GetMonthlyStatsAsync(selectedYear, selectedMonth, null);
            var projectBreakdown = await _timeLogRepository.GetProjectBreakdownAsync(selectedYear, selectedMonth, null);
            var userBreakdown = await _timeLogRepository.GetUserBreakdownAsync(selectedYear, selectedMonth);

            return new DashboardDto
            {
                Year = selectedYear,
                Month = selectedMonth,
                SelectedUserId = userId,
                Stats = stats,
                ProjectBreakdown = projectBreakdown.ToList(),
                UserBreakdown = userBreakdown.ToList(),
                CanViewAllUsers = true
            };
        }
    }
}