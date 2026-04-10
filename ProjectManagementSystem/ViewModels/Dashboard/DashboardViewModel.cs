namespace ProjectManagementSystem.ViewModels.Dashboard
{
    using DTOs.Dashboard;
    using Microsoft.AspNetCore.Mvc.Rendering;

    public class DashboardViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string? SelectedUserId { get; set; }
        public MonthlyStatsDto Stats { get; set; } = new();
        public List<ProjectTimeDto> ProjectBreakdown { get; set; } = new();
        public List<UserTimeDto> UserBreakdown { get; set; } = new();
        public List<SelectListItem> AvailableMonths { get; set; } = new();
        public List<SelectListItem> AvailableYears { get; set; } = new();
        public List<SelectListItem> AvailableUsers { get; set; } = new();
        public bool CanViewAllUsers { get; set; }
    }
}
