namespace ProjectManagementSystem.ViewModels.Tasks
{
    using Comments;
    using Enums;
    using Helpers;
    using TimeLogs;

    public class TaskDetailsViewModel
    {
        public int Id { get; set; }
        public string Tag { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskType Type { get; set; }
        public TaskPriority Priority { get; set; }
        public ProjectTaskStatus Status { get; set; }
        public DateTime? Deadline { get; set; }
        public string AssigneeEmail { get; set; }
        public string AssigneeName { get; set; }
        public int ProjectId { get; set; }
        public double TotalHours { get; set; }
        public IEnumerable<CommentListViewModel> Comments { get; set; }
        public IEnumerable<TimeLogListViewModel> TimeLogs { get; set; }
        public string FormattedTotalHours => TimeFormatter.Format(TotalHours);
        public string TypeIcon => TaskHelper.GetTypeIcon(Type);
    }
}