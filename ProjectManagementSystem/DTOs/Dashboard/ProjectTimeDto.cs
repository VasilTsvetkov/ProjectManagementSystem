namespace ProjectManagementSystem.DTOs.Dashboard
{
    public class ProjectTimeDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectTag { get; set; } = string.Empty;
        public double TotalHours { get; set; }
        public int TaskCount { get; set; }
        public int LogCount { get; set; }
    }
}