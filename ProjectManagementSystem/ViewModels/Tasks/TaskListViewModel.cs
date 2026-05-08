namespace ProjectManagementSystem.ViewModels.Tasks
{
    using Enums.Task;
    using Helpers;
    using System;
    using Type = Enums.Task.Type;

    public class TaskListViewModel
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

        public string TypeIcon => TaskHelper.GetTypeIcon(Type);
    }
}