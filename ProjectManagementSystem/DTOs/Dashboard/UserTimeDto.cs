namespace ProjectManagementSystem.DTOs.Dashboard
{
    public class UserTimeDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public double TotalHours { get; set; }
        public int ProjectCount { get; set; }
        public int TaskCount { get; set; }
    }
}
