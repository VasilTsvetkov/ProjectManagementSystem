namespace ProjectManagementSystem.Models
{
    using Enums;

    public class ActivityLog
    {
        public int Id { get; set; }

        public required string Message { get; set; }

        public required string UserId { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;

        public required ActivityType Type { get; set; }

        public required DateTime Timestamp { get; set; }
    }
}