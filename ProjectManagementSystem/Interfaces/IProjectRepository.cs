namespace ProjectManagementSystem.Interfaces
{
    using Models;

    public interface IProjectRepository : IRepository<Project>
    {
        Task<IEnumerable<Project>> GetProjectsByUserAsync(string userId);
        Task<bool> UpdateProjectAsync(int id, string name, string description);
        Task<bool> ExistsWithNameAsync(string name, int? excludeId = null);
    }
}