namespace ProjectManagementSystem.Models
{
    using Constants;
    using Enums;

    public class ProjectTask
    {
        public int Id { get; set; }
        public int TaskNumber { get; set; }
        public string Tag => $"{GetPrefix()}-{TaskNumber}";
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskType Type { get; set; }
        public TaskPriority Priority { get; set; }
        public ProjectTaskStatus Status { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public string? AssigneeId { get; set; }
        public ApplicationUser? Assignee { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<TimeLog> TimeLogs { get; set; }

        private string GetPrefix() => Type switch
        {
            TaskType.Bug => TaskConstants.BugPrefix,
            TaskType.Feature => TaskConstants.FeaturePrefix,
            TaskType.Task => TaskConstants.TaskPrefix,
            _ => TaskConstants.TaskPrefix
        };
    }
}