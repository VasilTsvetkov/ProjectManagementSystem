namespace ProjectManagementSystem.Services
{
    using Interfaces;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Globalization;
    using ViewModels.Dashboard;

    public class DashboardService(ITimeLogRepository timeLogRepository) : IDashboardService
    {
        private readonly ITimeLogRepository _timeLogRepository = timeLogRepository;

        public async Task<DashboardViewModel> GetDashboardDataAsync(int? year, int? month, string userId)
        {
            var selectedYear = year ?? DateTime.UtcNow.Year;
            var selectedMonth = month ?? DateTime.UtcNow.Month;

            var stats = await _timeLogRepository.GetMonthlyStatsAsync(selectedYear, selectedMonth, null);
            var projectBreakdown = await _timeLogRepository.GetProjectBreakdownAsync(selectedYear, selectedMonth, null);
            var userBreakdown = await _timeLogRepository.GetUserBreakdownAsync(selectedYear, selectedMonth);

            return new DashboardViewModel
            {
                Year = selectedYear,
                Month = selectedMonth,
                SelectedUserId = userId,
                Stats = stats,
                ProjectBreakdown = projectBreakdown.ToList(),
                UserBreakdown = userBreakdown.ToList(),
                AvailableMonths = GetMonthSelectList(),
                AvailableYears = GetYearSelectList(),
                AvailableUsers = userBreakdown.Select(u => new SelectListItem
                {
                    Value = u.UserId,
                    Text = u.UserName
                }).ToList(),
                CanViewAllUsers = true
            };
        }

        private static List<SelectListItem> GetMonthSelectList()
        {
            return Enumerable.Range(1, 12)
                .Select(m => new SelectListItem
                {
                    Value = m.ToString(),
                    Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)
                })
                .ToList();
        }

        private static List<SelectListItem> GetYearSelectList()
        {
            var currentYear = DateTime.UtcNow.Year;

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