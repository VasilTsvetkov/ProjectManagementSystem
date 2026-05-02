namespace ProjectManagementSystem.Services
{
    using Interfaces;
    using Models;
    using ViewModels.Projects;
    using Microsoft.Extensions.Logging;

    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(IProjectRepository projectRepository, ILogger<ProjectService> logger)
        {
            _projectRepository = projectRepository;
            _logger = logger;
        }

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

            _logger.LogInformation("Project {ProjectName} created by user {UserId}", model.Name, creatorId);

            return true;
        }

        public async Task<bool> UpdateProjectAsync(int id, ProjectViewModel model)
        {
            var updated = await _projectRepository.UpdateProjectAsync(id, model.Name, model.Description);

            if (updated)
            {
                _logger.LogInformation("Project {ProjectId} updated", id);
            }

            return updated;
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var deleted = await _projectRepository.DeleteAsync(id);

            if (deleted)
            {
                _logger.LogInformation("Project {ProjectId} deleted", id);
            }

            return deleted;
        }
    }
}