namespace ProjectManagementSystem.Models
{
    using Enums;

    public class ActivityLog
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public ActivityType Type { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
