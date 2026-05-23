namespace ProjectManagementSystem.Web.ViewModels.TimeLogs
{
    using System;
    using System.Collections.Generic;

    public class MonthlyMatrixViewModel
    {
        public int ProjectId { get; init; }

        public required string ProjectName { get; init; }

        public DateTime SelectedMonth { get; init; }

        public IReadOnlyList<DateTime> DaysInMonth { get; init; } = [];

        public IReadOnlyList<UserMatrixRowViewModel> Rows { get; init; } = [];
    }
}