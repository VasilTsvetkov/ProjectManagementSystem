namespace ProjectManagementSystem.Interfaces
{
    using DTOs;
    using Models;

    public interface ITaskRepository : IRepository<ProjectTask>
    {
        Task<IEnumerable<ProjectTask>> GetTasksByProjectAsync(int projectId);
        Task<IEnumerable<ProjectTask>> GetTasksByAssigneeAsync(string userId);
        Task AddAsync(ProjectTask entity);
        Task<ProjectTask?> GetTaskByIdAsync(int id);
        Task<bool> UpdateTaskAsync(int id, UpdateTaskDto dto);
    }
}