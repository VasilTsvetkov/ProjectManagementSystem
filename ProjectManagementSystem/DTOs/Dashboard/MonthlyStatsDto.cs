namespace ProjectManagementSystem.DTOs.Dashboard
{
    public class MonthlyStatsDto
    {
        public required double TotalHours { get; init; }

        public required int TotalProjects { get; init; }

        public required int TotalTasks { get; init; }

        public required int TotalLogs { get; init; }
    }
}