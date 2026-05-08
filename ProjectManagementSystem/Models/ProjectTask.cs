namespace ProjectManagementSystem.Models
{
    using Constants;
    using Enums.Task;

    public class ProjectTask
    {
        public int Id { get; set; }

        public int Number { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public required Type Type { get; set; }

        public required Priority Priority { get; set; }

        public required Status Status { get; set; }

        public DateTime? Deadline { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required int ProjectId { get; set; }

        public virtual Project Project { get; set; } = null!;

        public string? AssigneeId { get; set; }

        public virtual ApplicationUser? Assignee { get; set; }

        public required string ReporterId { get; set; }

        public virtual ApplicationUser Reporter { get; set; } = null!;

        public virtual ICollection<Comment> Comments { get; set; } = [];

        public virtual ICollection<TimeLog> TimeLogs { get; set; } = [];

        public string Tag => $"{GetPrefix()}-{Number}";

        private string GetPrefix() => Type switch
        {
            Type.Bug => TaskConstants.BugPrefix,
            Type.Feature => TaskConstants.FeaturePrefix,
            Type.Task => TaskConstants.TaskPrefix,
            _ => TaskConstants.TaskPrefix
        };
    }
}