namespace ProjectManagementSystem.Common.DTOs.Tasks
{
    using Common.DTOs.TimeLogs;
    using Common.Enums.Task;
    using System;
    using System.Collections.Generic;
    using Type = Enums.Task.Type;

    public class TaskDetailsDto
    {
        public int Id { get; init; }
        public int ProjectId { get; init; }
        public required string Tag { get; init; }
        public required string Title { get; init; }
        public string? Description { get; init; }
        public required Status Status { get; init; }
        public required Priority Priority { get; init; }
        public required Type Type { get; init; }
        public DateTime? Deadline { get; init; }
        public required string AssigneeName { get; init; }
        public required string ReporterName { get; init; }
        public double TotalHours { get; init; }
        public IReadOnlyList<CommentDto> Comments { get; init; } = [];
        public IReadOnlyList<TimeLogDto> TimeLogs { get; init; } = [];
    }
}