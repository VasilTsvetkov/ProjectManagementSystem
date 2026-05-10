namespace ProjectManagementSystem.Services
{
    using Constants;
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

        public async Task<IEnumerable<ProjectDisplayViewModel>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllAsync();

            return projects.Select(p => new ProjectDisplayViewModel
            {
                Id = p.Id,
                Number = p.Number,
                Tag = p.Tag,
                Name = p.Name ?? MessageConstants.UntitledProject,
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
                Name = project.Name ?? MessageConstants.UntitledProject,
                Description = project.Description
            };
        }

        public async Task<ProjectDisplayViewModel?> GetProjectForDeleteAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
            {
                return null;
            }

            return new ProjectDisplayViewModel
            {
                Id = project.Id,
                Number = project.Number,
                Tag = project.Tag,
                Name = project.Name ?? MessageConstants.UntitledProject,
                Description = project.Description,
                CreatedAt = project.CreatedAt
            };
        }

        public async Task<bool> CreateProjectAsync(ProjectViewModel model, string creatorId)
        {
            var projectName = string.IsNullOrWhiteSpace(model.Name)
                ? MessageConstants.UntitledProject
                : model.Name;

            var project = new Project
            {
                Name = projectName,
                Description = model.Description,
                CreatorId = creatorId,
                CreatedAt = DateTime.UtcNow
            };

            await _projectRepository.AddAsync(project);

            await _activityService.LogAsync(
                creatorId,
                string.Format(MessageConstants.ActivityCreatedProject, projectName),
                ActivityType.ProjectAction);

            _logger.LogInformation("Project {ProjectName} created by user {UserId}", projectName, creatorId);

            return true;
        }

        public async Task<bool> UpdateProjectAsync(int id, ProjectViewModel model, string userId)
        {
            var projectName = string.IsNullOrWhiteSpace(model.Name)
                ? MessageConstants.UntitledProject
                : model.Name;

            var updated = await _projectRepository.UpdateProjectAsync(id, projectName, model.Description);

            if (updated)
            {
                await _activityService.LogAsync(
                    userId,
                    string.Format(MessageConstants.ActivityUpdatedProject, projectName),
                    ActivityType.ProjectAction);

                _logger.LogInformation("Project {ProjectId} updated by user {UserId}", id, userId);
            }

            return updated;
        }

        public async Task<bool> DeleteProjectAsync(int id, string userId)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            var projectName = project?.Name ?? MessageConstants.UnknownProject;

            var deleted = await _projectRepository.DeleteAsync(id);

            if (deleted)
            {
                await _activityService.LogAsync(
                    userId,
                    string.Format(MessageConstants.ActivityDeletedProject, projectName),
                    ActivityType.ProjectAction);

                _logger.LogInformation("Project {ProjectId} ({ProjectName}) deleted by user {UserId}", id, projectName, userId);
            }

            return deleted;
        }
    }
}