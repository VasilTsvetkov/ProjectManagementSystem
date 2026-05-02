namespace ProjectManagementSystem.ViewModels.Dashboard
{
    using DTOs.Dashboard;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Collections.Generic;

    public class DashboardViewModel
    {
        public int Year { get; set; } = DateTime.Now.Year;
        public int Month { get; set; } = DateTime.Now.Month;
        public string? SelectedUserId { get; set; }
        public MonthlyStatsDto Stats { get; set; } = new();
        public List<ProjectTimeDto> ProjectBreakdown { get; set; } = [];
        public List<UserTimeDto> UserBreakdown { get; set; } = [];
        public List<SelectListItem> AvailableMonths { get; set; } = [];
        public List<SelectListItem> AvailableYears { get; set; } = [];
        public List<SelectListItem> AvailableUsers { get; set; } = [];
        public bool CanViewAllUsers { get; set; }
    }
}