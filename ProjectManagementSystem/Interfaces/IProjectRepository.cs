namespace ProjectManagementSystem.Interfaces
{
    using Models;

    public interface IProjectRepository : IRepository<Project>
    {
        new Task AddAsync(Project entity);
        Task<bool> UpdateProjectAsync(int id, string name, string description);
    }
}