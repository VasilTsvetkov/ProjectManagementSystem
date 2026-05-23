namespace ProjectManagementSystem.Core.Interfaces
{
    using Common.DTOs.Tasks;
    using Common.Enums.Task;
    using Models;

    public interface ITaskRepository : IRepository<ProjectTask>
    {
        Task<IEnumerable<ProjectTask>> GetTasksByProjectAsync(int projectId);

        Task<IEnumerable<ProjectTask>> GetTasksByAssigneeAsync(string userId);

        Task<bool> UpdateTaskAsync(int id, UpdateTaskDto dto);

        Task<bool> UpdateStatusAsync(int id, Status status);
    }
}