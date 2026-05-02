namespace ProjectManagementSystem.ViewModels.Tasks
{
    using Comments;
    using Enums;
    using Helpers;
    using TimeLogs;
    using System.Collections.Generic;

    public class TaskDetailsViewModel
    {
        public int Id { get; set; }
        public string Tag { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskType Type { get; set; }
        public TaskPriority Priority { get; set; }
        public ProjectTaskStatus Status { get; set; }
        public DateTime? Deadline { get; set; }
        public string AssigneeName { get; set; } = string.Empty;
        public string ReporterName { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public double TotalHours { get; set; }
        public IEnumerable<CommentListViewModel> Comments { get; set; } = [];
        public IEnumerable<TimeLogListViewModel> TimeLogs { get; set; } = [];
        public string FormattedTotalHours => TimeFormatter.Format(TotalHours);
    }
}