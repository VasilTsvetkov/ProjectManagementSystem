namespace ProjectManagementSystem.Web.ViewModels.Tasks
{
    using BL.Enums.Task;
    using BL.Helpers;
    using Comments;
    using System;
    using System.Collections.Generic;
    using TimeLogs;
    using Type = BL.Enums.Task.Type;

    public class TaskDetailsViewModel
    {
        public int Id { get; init; }

        public required string Tag { get; init; }

        public required string Title { get; init; }

        public string? Description { get; init; }

        public required Type Type { get; init; }

        public required Priority Priority { get; init; }

        public required Status Status { get; init; }

        public DateTime? Deadline { get; init; }

        public required string AssigneeName { get; init; }

        public required string ReporterName { get; init; }

        public int ProjectId { get; init; }

        public double TotalHours { get; init; }

        public IReadOnlyList<CommentListViewModel> Comments { get; init; } = [];

        public IReadOnlyList<TimeLogListViewModel> TimeLogs { get; init; } = [];

        public string FormattedTotalHours => TimeFormatter.Format(TotalHours);

        public string TypeIcon => TaskHelper.GetTypeIcon(Type);
    }
}