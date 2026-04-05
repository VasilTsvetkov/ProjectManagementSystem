namespace ProjectManagementSystem.ViewModels.Tasks
{
    using Constants;
    using Enums;
    using ProjectManagementSystem.Helpers;

    public class TaskListViewModel
    {
        public int Id { get; set; }
        public string Tag { get; set; }
        public string Title { get; set; }
        public TaskType Type { get; set; }
        public TaskPriority Priority { get; set; }
        public ProjectTaskStatus Status { get; set; }
        public DateTime? Deadline { get; set; }
        public string AssigneeEmail { get; set; }
        public string AssigneeName { get; set; }
        public string TypeIcon => TaskHelper.GetTypeIcon(Type);
    }
}