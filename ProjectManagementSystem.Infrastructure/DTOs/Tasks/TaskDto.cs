namespace ProjectManagementSystem.BL.DTOs.Tasks
{
    using Enums.Task;
    using System;
    using System.Collections.Generic;
    using Type = Enums.Task.Type;

    public class TaskDto
    {
        public int Id { get; init; }
        public int ProjectId { get; init; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public Type Type { get; set; }
        public Priority Priority { get; set; }
        public Status Status { get; set; }
        public string? Tag { get; set; }
        public DateTime? Deadline { get; set; }
        public string? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public List<UserDto> Users { get; set; } = [];
    }
}