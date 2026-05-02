namespace ProjectManagementSystem.Services
{
    using DTOs.Dashboard;
    using Interfaces;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.Extensions.Logging;
    using System.Globalization;
    using ViewModels.Dashboard;

    public class DashboardService : IDashboardService
    {
        private readonly ITimeLogRepository _timeLogRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(ITimeLogRepository timeLogRepository, ILogger<DashboardService> logger)
        {
            _timeLogRepository = timeLogRepository;
            _logger = logger;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(int? year, int? month, string userId)
        {
            try
            {
                _logger.LogInformation("Fetching dashboard stats for user {UserId}", userId);

                var selectedYear = year ?? DateTime.Now.Year;
                var selectedMonth = month ?? DateTime.Now.Month;

                var stats = await _timeLogRepository.GetMonthlyStatsAsync(selectedYear, selectedMonth, userId);
                var projectBreakdown = await _timeLogRepository.GetProjectBreakdownAsync(selectedYear, selectedMonth, userId);

                return new DashboardViewModel
                {
                    Year = selectedYear,
                    Month = selectedMonth,
                    SelectedUserId = userId,
                    Stats = stats,
                    ProjectBreakdown = projectBreakdown.ToList(),
                    UserBreakdown = new List<UserTimeDto>(),
                    AvailableMonths = GetMonthSelectList(),
                    AvailableYears = GetYearSelectList(),
                    AvailableUsers = new List<SelectListItem>(),
                    CanViewAllUsers = false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard data for user {UserId}", userId);
                throw; 
            }
        }

        private List<SelectListItem> GetMonthSelectList()
        {
            return Enumerable.Range(1, 12)
                .Select(m => new SelectListItem
                {
                    Value = m.ToString(),
                    Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)
                })
                .ToList();
        }

        private List<SelectListItem> GetYearSelectList()
        {
            var currentYear = DateTime.Now.Year;
            return Enumerable.Range(currentYear - 2, 5)
                .Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString()
                })
                .ToList();
        }
    }
}