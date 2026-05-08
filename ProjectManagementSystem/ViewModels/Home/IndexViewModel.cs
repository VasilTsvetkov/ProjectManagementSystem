namespace ProjectManagementSystem.ViewModels.Home
{
    using DTOs;
    using System.Collections.Generic;
    using Tasks;

    public class IndexViewModel
    {
        public int MyPendingTasksCount { get; init; }

        public int OverdueTasksCount { get; init; }

        public required IReadOnlyList<TaskListViewModel> UrgentTasks { get; init; }

        public required IReadOnlyList<ActivityDto> RecentActivities { get; init; }
    }
}