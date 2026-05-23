namespace ProjectManagementSystem.Common.DTOs.TimeLogs;

public class UserMatrixRowDto
{
    public required string UserId { get; init; }
    public required string FullName { get; init; }
    public IReadOnlyDictionary<int, double> DailyHours { get; init; } = new Dictionary<int, double>();
}