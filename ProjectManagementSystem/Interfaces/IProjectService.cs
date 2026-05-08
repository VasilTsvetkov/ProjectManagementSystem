namespace ProjectManagementSystem.Interfaces
{
    using ViewModels.Projects;

    public interface IProjectService
    {
        Task<IEnumerable<ProjectDisplayViewModel>> GetAllProjectsAsync();

        Task<ProjectViewModel?> GetProjectForEditAsync(int id);

        Task<ProjectDisplayViewModel?> GetProjectForDeleteAsync(int id);

        Task<bool> CreateProjectAsync(ProjectViewModel model, string createdByUserId);

        Task<bool> UpdateProjectAsync(int id, ProjectViewModel model, string userId);

        Task<bool> DeleteProjectAsync(int id, string userId);
    }
}