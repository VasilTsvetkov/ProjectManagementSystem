namespace ProjectManagementSystem.Common.DTOs.Tasks
{
    using System;
    using Enums.Task;
    using Type = Enums.Task.Type;

    public class UpdateTaskDto
    {
        public required string Title { get; init; }

        public string? Description { get; init; }

        public required Type Type { get; init; }

        public required Priority Priority { get; init; }

        public required Status Status { get; init; }

        public DateTime? Deadline { get; init; }

        public string? AssigneeId { get; init; }
    }
}