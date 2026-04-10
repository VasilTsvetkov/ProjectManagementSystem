namespace ProjectManagementSystem.Interfaces
{
    using DTOs.Dashboard;
    using Models;

    public interface ITimeLogRepository : IRepository<TimeLog>
    {
        Task<IEnumerable<TimeLog>> GetTimeLogsByTaskAsync(int taskId);
        Task<IEnumerable<TimeLog>> GetTimeLogsByUserAsync(string userId);
        Task<IEnumerable<TimeLog>> GetTimeLogsByProjectAsync(int projectId);
        Task<IEnumerable<TimeLog>> GetByMonthAsync(int year, int month, string? userId = null);
        Task<MonthlyStatsDto> GetMonthlyStatsAsync(int year, int month, string? userId = null);
        Task<IEnumerable<ProjectTimeDto>> GetProjectBreakdownAsync(int year, int month, string? userId = null);
        Task<IEnumerable<UserTimeDto>> GetUserBreakdownAsync(int year, int month, int? projectId = null);
    }
}