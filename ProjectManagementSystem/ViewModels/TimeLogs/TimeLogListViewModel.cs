using ProjectManagementSystem.Helpers;

namespace ProjectManagementSystem.ViewModels.TimeLogs
{
    public class TimeLogListViewModel
    {
        public int Id { get; set; }
        public double Hours { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public string UserName { get; set; }
        public bool CanEdit { get; set; }
        public string FormattedHours => TimeFormatter.Format(Hours);
    }
}