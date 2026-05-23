namespace ProjectManagementSystem.Web.ViewModels.TimeLogs
{
    using BL.Helpers;

    public class TimeLogListViewModel
    {
        public int Id { get; init; }

        public double Hours { get; init; }

        public DateTime Date { get; init; }

        public string? Description { get; init; }

        public required string UserName { get; init; }

        public bool CanEdit { get; init; }

        public string FormattedHours => TimeFormatter.Format(Hours);
    }
}