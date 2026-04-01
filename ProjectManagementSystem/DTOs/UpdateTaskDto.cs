namespace ProjectManagementSystem.DTOs
{
    using Enums;

    public class UpdateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskType Type { get; set; }
        public TaskPriority Priority { get; set; }
        public ProjectTaskStatus Status { get; set; }
        public DateTime? Deadline { get; set; }
        public string AssigneeId { get; set; }
    }
}