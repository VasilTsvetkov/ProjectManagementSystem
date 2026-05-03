namespace ProjectManagementSystem.ViewModels.TimeLogs
{
    public class UserMatrixRowViewModel
    {
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public Dictionary<int, double> DailyHours { get; set; } = [];
        public double TotalHours => DailyHours.Values.Sum();
    }
}