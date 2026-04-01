namespace ProjectManagementSystem.Interfaces
{
    using Models;

    public interface IProjectRepository : IRepository<Project>
    {
        Task AddAsync(Project entity);
        Task<IEnumerable<Project>> GetProjectsByUserAsync(string userId);
        Task<bool> UpdateProjectAsync(int id, string name, string description);
    }
}