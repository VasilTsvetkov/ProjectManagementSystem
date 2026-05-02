namespace ProjectManagementSystem.ViewModels.Tasks
{
    using Enums;
    using Helpers;

    public class TaskListViewModel
    {
        public int Id { get; set; }
        public string Tag { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public TaskType Type { get; set; }
        public TaskPriority Priority { get; set; }
        public ProjectTaskStatus Status { get; set; }
        public DateTime? Deadline { get; set; }
        public string AssigneeName { get; set; } = string.Empty;
        public string TypeIcon => TaskHelper.GetTypeIcon(Type);
    }
}