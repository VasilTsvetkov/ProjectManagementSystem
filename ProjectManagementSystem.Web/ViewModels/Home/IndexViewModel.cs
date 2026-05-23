namespace ProjectManagementSystem.Web.ViewModels.Home
{
    using BL.DTOs;
    using BL.DTOs.Tasks;
    using System.Collections.Generic;

    public class IndexViewModel
    {
        public int MyPendingTasksCount { get; init; }

        public int OverdueTasksCount { get; init; }

        public required IReadOnlyList<TaskDto> UrgentTasks { get; init; }

        public required IReadOnlyList<ActivityDto> RecentActivities { get; init; }
    }
}