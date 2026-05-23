namespace ProjectManagementSystem.BL.DTOs
{
    using Tasks;

    public class HomeIndexDto
    {
        public int MyPendingTasksCount { get; init; }
        public int OverdueTasksCount { get; init; }
        public required IReadOnlyList<TaskDto> UrgentTasks { get; init; }
        public required IReadOnlyList<ActivityDto> RecentActivities { get; init; }
    }
}