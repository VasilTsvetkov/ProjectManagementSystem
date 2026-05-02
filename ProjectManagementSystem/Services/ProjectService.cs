namespace ProjectManagementSystem.Services
{
    using Interfaces;
    using Models;
    using ViewModels.Projects;

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
            try
            {
                _logger.LogInformation("Fetching all projects");

                var projects = await _projectRepository.GetAllAsync();

                var viewModels = projects.Select(p => new ProjectListViewModel
                {
                    Id = p.Id,
                    Tag = p.Tag,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt
                }).ToList();

                _logger.LogInformation("Fetched {Count} projects", viewModels.Count);
                return viewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all projects");
                throw;
            }
        }

        public async Task<ProjectViewModel?> GetProjectForEditAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching project {ProjectId} for edit", id);

                var project = await _projectRepository.GetByIdAsync(id);
                if (project == null)
                {
                    _logger.LogWarning("Project {ProjectId} not found", id);
                    return null;
                }

                return new ProjectViewModel
                {
                    Name = project.Name,
                    Description = project.Description
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching project {ProjectId} for edit", id);
                throw;
            }
        }

        public async Task<ProjectDetailsViewModel?> GetProjectForDeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching project {ProjectId} for delete", id);

                var project = await _projectRepository.GetByIdAsync(id);
                if (project == null)
                {
                    _logger.LogWarning("Project {ProjectId} not found", id);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching project {ProjectId} for delete", id);
                throw;
            }
        }

        public async Task<bool> CreateProjectAsync(ProjectViewModel model, string creatorId)
        {
            try
            {
                _logger.LogInformation("Creating project {ProjectName} by user {UserId}", model.Name, creatorId);

                var project = new Project
                {
                    Name = model.Name,
                    Description = model.Description,
                    CreatorId = creatorId,
                    CreatedAt = DateTime.UtcNow
                };

                await _projectRepository.AddAsync(project);

                _logger.LogInformation("Project {ProjectName} created successfully with ID {ProjectId}", model.Name, project.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project {ProjectName}", model.Name);
                throw;
            }
        }

        public async Task<bool> UpdateProjectAsync(int id, ProjectViewModel model)
        {
            try
            {
                _logger.LogInformation("Updating project {ProjectId}", id);

                var updated = await _projectRepository.UpdateProjectAsync(id, model.Name, model.Description);

                if (updated)
                {
                    _logger.LogInformation("Project {ProjectId} updated successfully", id);
                }
                else
                {
                    _logger.LogWarning("Project {ProjectId} not found for update", id);
                }

                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project {ProjectId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting project {ProjectId}", id);

                var deleted = await _projectRepository.DeleteAsync(id);

                if (deleted)
                {
                    _logger.LogInformation("Project {ProjectId} deleted successfully", id);
                }
                else
                {
                    _logger.LogWarning("Project {ProjectId} not found for deletion", id);
                }

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project {ProjectId}", id);
                throw;
            }
        }
    }
}