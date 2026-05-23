namespace ProjectManagementSystem.BL.DTOs.TimeLogs
{
    public class MonthlyMatrixDto
    {
        public int ProjectId { get; init; }
        public required string ProjectName { get; init; }
        public DateTime SelectedMonth { get; init; }
        public IReadOnlyList<DateTime> DaysInMonth { get; init; } = [];
        public IReadOnlyList<UserMatrixRowDto> Rows { get; init; } = [];
    }
}