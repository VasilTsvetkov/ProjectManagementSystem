namespace ProjectManagementSystem.DTOs.Dashboard
{
    public class UserTimeDto
    {
        public required string UserId { get; init; }

        public required string UserName { get; init; }

        public required double TotalHours { get; init; }

        public required int ProjectCount { get; init; }

        public required int TaskCount { get; init; }
    }
}