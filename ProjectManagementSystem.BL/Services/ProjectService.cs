namespace ProjectManagementSystem.BL.Services
{
    using Constants;
    using DTOs.Projects;
    using Enums;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Models;

    public class ProjectService(
        IProjectRepository projectRepository,
        IActivityService activityService,
        ILogger<ProjectService> logger) : IProjectService
    {
        private readonly IProjectRepository _projectRepository = projectRepository;
        private readonly IActivityService _activityService = activityService;
        private readonly ILogger<ProjectService> _logger = logger;

        public async Task<IEnumerable<ProjectDisplayDto>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllAsync();

            return projects.Select(p => new ProjectDisplayDto
            {
                Id = p.Id,
                Number = p.Number,
                Tag = p.Tag,
                Name = p.Name ?? MessageConstants.UntitledProject,
                Description = p.Description,
                CreatedAt = p.CreatedAt
            }).ToList();
        }

        public async Task<ProjectDto?> GetProjectForEditAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
            {
                return null;
            }

            return new ProjectDto
            {
                Name = project.Name ?? MessageConstants.UntitledProject,
                Description = project.Description
            };
        }

        public async Task<ProjectDisplayDto?> GetProjectForDeleteAsync(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);

            if (project == null)
            {
                return null;
            }

            return new ProjectDisplayDto
            {
                Id = project.Id,
                Number = project.Number,
                Tag = project.Tag,
                Name = project.Name ?? MessageConstants.UntitledProject,
                Description = project.Description,
                CreatedAt = project.CreatedAt
            };
        }

        public async Task<bool> CreateProjectAsync(ProjectDto model, string creatorId)
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

        public async Task<bool> UpdateProjectAsync(int id, ProjectDto model, string userId)
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