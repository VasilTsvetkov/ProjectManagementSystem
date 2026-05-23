namespace ProjectManagementSystem.Common.DTOs.Tasks
{
    using Common.Enums.Task;
    using ProjectManagementSystem.Common.DTOs;

    public class TaskDto
    {
        public int Id { get; init; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public Type Type { get; set; }
        public Priority Priority { get; set; }
        public Status Status { get; set; }
        public string? Tag { get; set; }
        public DateTime? Deadline { get; set; }
        public string? AssigneeId { get; set; }
        public List<UserDto> Users { get; set; } = [];
    }
}