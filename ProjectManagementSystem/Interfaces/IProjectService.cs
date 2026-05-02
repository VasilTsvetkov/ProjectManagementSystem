namespace ProjectManagementSystem.Interfaces
{
    using ViewModels.Projects;

    public interface IProjectService
    {
        Task<IEnumerable<ProjectListViewModel>> GetAllProjectsAsync();
        Task<ProjectViewModel?> GetProjectForEditAsync(int id);
        Task<ProjectDetailsViewModel?> GetProjectForDeleteAsync(int id);
        Task<bool> CreateProjectAsync(ProjectViewModel model, string createdByUserId);
        Task<bool> UpdateProjectAsync(int id, ProjectViewModel model);
        Task<bool> DeleteProjectAsync(int id);
    }
}