namespace ProjectManagementSystem.Web.ViewModels.Dashboard
{
    using BL.DTOs.Dashboard;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Collections.Generic;

    public class DashboardViewModel
    {
        public int Year { get; init; }

        public int Month { get; init; }

        public string? SelectedUserId { get; init; }

        public required MonthlyStatsDto Stats { get; init; }

        public required IReadOnlyList<ProjectTimeDto> ProjectBreakdown { get; init; }

        public required IReadOnlyList<UserTimeDto> UserBreakdown { get; init; }

        public required IReadOnlyList<SelectListItem> AvailableMonths { get; init; }

        public required IReadOnlyList<SelectListItem> AvailableYears { get; init; }

        public required IReadOnlyList<SelectListItem> AvailableUsers { get; init; }

        public bool CanViewAllUsers { get; init; }
    }
}