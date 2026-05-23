namespace ProjectManagementSystem.BL.DTOs.Tasks
{
    using Enums.Task;

    public class TaskListDto
    {
        public int Id { get; init; }
        public int ProjectId { get; init; }
        public required string Tag { get; init; }
        public required string Title { get; init; }
        public required Type Type { get; init; }
        public required Priority Priority { get; init; }
        public required Status Status { get; init; }
        public DateTime? Deadline { get; init; }
        public string? AssigneeName { get; init; }
    }
}