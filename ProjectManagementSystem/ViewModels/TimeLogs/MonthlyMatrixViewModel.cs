namespace ProjectManagementSystem.ViewModels.TimeLogs
{
    public class MonthlyMatrixViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;
        public DateTime SelectedMonth { get; set; }
        public List<DateTime> DaysInMonth { get; set; } = [];
        public List<UserMatrixRowViewModel> Rows { get; set; } = [];
    }
}