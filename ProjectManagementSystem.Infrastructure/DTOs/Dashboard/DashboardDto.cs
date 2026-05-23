namespace ProjectManagementSystem.BL.DTOs.Dashboard
{
    public class DashboardDto
    {
        public int Year { get; init; }
        public int Month { get; init; }
        public string? SelectedUserId { get; init; }
        public required MonthlyStatsDto Stats { get; init; }
        public required IReadOnlyList<ProjectTimeDto> ProjectBreakdown { get; init; }
        public required IReadOnlyList<UserTimeDto> UserBreakdown { get; init; }
        public bool CanViewAllUsers { get; init; }
    }
}