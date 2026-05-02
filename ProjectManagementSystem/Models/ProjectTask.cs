namespace ProjectManagementSystem.Models
{
    using Constants;
    using Enums;

    public class ProjectTask
    {
        public ProjectTask()
        {
            Comments = new List<Comment>();
            TimeLogs = new List<TimeLog>();
        }

        public int Id { get; set; }
        public int TaskNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskType Type { get; set; }
        public TaskPriority Priority { get; set; }
        public ProjectTaskStatus Status { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public string? AssigneeId { get; set; }
        public ApplicationUser? Assignee { get; set; }
        public string ReporterId { get; set; } = string.Empty;
        public ApplicationUser? Reporter { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<TimeLog> TimeLogs { get; set; }
        public string Tag => $"{GetPrefix()}-{TaskNumber}";

        private string GetPrefix() => Type switch
        {
            TaskType.Bug => TaskConstants.BugPrefix,
            TaskType.Feature => TaskConstants.FeaturePrefix,
            TaskType.Task => TaskConstants.TaskPrefix,
            _ => TaskConstants.TaskPrefix
        };
    }
}