namespace ProjectManagementSystem.Core.Interfaces
{
    using Common.DTOs.Projects;

    public interface IProjectService
    {
        Task<IEnumerable<ProjectDisplayDto>> GetAllProjectsAsync();

        Task<ProjectDto?> GetProjectForEditAsync(int id);

        Task<ProjectDisplayDto?> GetProjectForDeleteAsync(int id);

        Task<bool> CreateProjectAsync(ProjectDto model, string createdByUserId);

        Task<bool> UpdateProjectAsync(int id, ProjectDto model, string userId);

        Task<bool> DeleteProjectAsync(int id, string userId);
    }
}