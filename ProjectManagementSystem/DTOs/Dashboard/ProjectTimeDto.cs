namespace ProjectManagementSystem.DTOs.Dashboard
{
    public class ProjectTimeDto
    {
        public required int ProjectId { get; init; }

        public required string ProjectName { get; init; }

        public required string ProjectTag { get; init; }

        public required double TotalHours { get; init; }

        public required int TaskCount { get; init; }

        public required int LogCount { get; init; }
    }
}