namespace ProjectManagementSystem.Core.Interfaces
{
    using Common.DTOs.TimeLogs;

    public interface ITimeLogService
    {
        Task<bool> CreateTimeLogAsync(TimeLogDto dto, string userId);

        Task<(bool Success, int TaskId)?> DeleteTimeLogAsync(int id, string userId);

        Task<MonthlyMatrixDto?> GetMonthlyMatrixAsync(int projectId, int month, int year);
    }
}