namespace ProjectManagementSystem.ViewModels.Home
{
    using Enums;

    public class ActivityDto
    {
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public ActivityType Type { get; set; }
    }
}