namespace ProjectManagementSystem.ViewModels.Home
{
    using Tasks;

    public class IndexViewModel
    {
        public int MyPendingTasksCount { get; set; }
        public int OverdueTasksCount { get; set; }
        public List<TaskListViewModel> UrgentTasks { get; set; } = [];
        public List<ActivityDto> RecentActivities { get; set; } = [];
    }
}