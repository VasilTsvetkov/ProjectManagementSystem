namespace ProjectManagementSystem.Interfaces
{
    using DTOs;
    using Models;
    using Enums;

    public interface ITaskRepository : IRepository<ProjectTask>
    {
        Task<IEnumerable<ProjectTask>> GetTasksByProjectAsync(int projectId);
        Task<IEnumerable<ProjectTask>> GetTasksByAssigneeAsync(string userId);
        Task<bool> UpdateTaskAsync(int id, UpdateTaskDto dto);
        Task<bool> UpdateStatusAsync(int id, ProjectTaskStatus status);
    }
}