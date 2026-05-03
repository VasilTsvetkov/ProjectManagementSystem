namespace ProjectManagementSystem.Services
{
    using Enums;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Models;
    using ViewModels.Projects;

    public class ProjectService(
        IProjectRepository projectRepository,
        IActivityService activityService,
        ILogger<ProjectService> logger) : IProjectService
    {
        private readonly IProjectRepository _projectRepository = projectRepository;
        private readonly IActivityService _activityService = activityService;
        private readonly ILogger<ProjectService> _logger = logger;

        public async Task<IEnumerable<ProjectListViewModel>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllAsync();

            return projects.Select(p => new ProjectListViewModel
            {
                Id = p.Id,
                Tag = p.Tag,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt
            }).ToList();
        }

        public async Task<ProjectViewModel?> GetProjectForEditAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
            {
                return null;
            }

            return new ProjectViewModel
            {
                Name = project.Name,
                Description = project.Description
            };
        }

        public async Task<ProjectDetailsViewModel?> GetProjectForDeleteAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
            {
                return null;
            }

            return new ProjectDetailsViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt
            };
        }

        public async Task<bool> CreateProjectAsync(ProjectViewModel model, string creatorId)
        {
            var project = new Project
            {
                Name = model.Name,
                Description = model.Description,
                CreatorId = creatorId,
                CreatedAt = DateTime.UtcNow
            };

            await _projectRepository.AddAsync(project);

            await _activityService.LogAsync(creatorId, $"Created project: {model.Name}", ActivityType.ProjectAction);

            _logger.LogInformation("Project {ProjectName} created by user {UserId}", model.Name, creatorId);

            return true;
        }

        public async Task<bool> UpdateProjectAsync(int id, ProjectViewModel model, string userId)
        {
            var updated = await _projectRepository.UpdateProjectAsync(id, model.Name, model.Description);

            if (updated)
            {
                await _activityService.LogAsync(userId, $"Updated details for project: {model.Name}", ActivityType.ProjectAction);
                _logger.LogInformation("Project {ProjectId} updated", id);
            }

            return updated;
        }

        public async Task<bool> DeleteProjectAsync(int id, string userId)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            var projectName = project?.Name ?? "Unknown Project";

            var deleted = await _projectRepository.DeleteAsync(id);

            if (deleted)
            {
                await _activityService.LogAsync(userId, $"Deleted project: {projectName}", ActivityType.ProjectAction);
                _logger.LogInformation("Project {ProjectId} deleted", id);
            }

            return deleted;
        }
    }
}