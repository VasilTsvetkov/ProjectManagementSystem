namespace ProjectManagementSystem.Interfaces
{
    using Models;

    public interface ITimeLogRepository : IRepository<TimeLog>
    {
        Task<IEnumerable<TimeLog>> GetTimeLogsByTaskAsync(int taskId);
        Task<IEnumerable<TimeLog>> GetTimeLogsByUserAsync(string userId);
        Task<IEnumerable<TimeLog>> GetTimeLogsByProjectAsync(int projectId);
    }
}