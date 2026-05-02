namespace ProjectManagementSystem.Interfaces
{
    using ViewModels.TimeLogs;

    public interface ITimeLogService
    {
        Task<bool> CreateTimeLogAsync(TimeLogViewModel model, string userId);
        Task<(bool Success, int TaskId)?> DeleteTimeLogAsync(int id, string userId);
    }
}