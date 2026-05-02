namespace ProjectManagementSystem.Services
{
    using DTOs;
    using Enums;
    using Helpers;
    using Interfaces;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Models;
    using ViewModels.Comments;
    using ViewModels.Tasks;
    using ViewModels.TimeLogs;

    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly ITimeLogRepository _timeLogRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<TaskService> _logger;

        public TaskService(
            ITaskRepository taskRepository,
            IProjectRepository projectRepository,
            ICommentRepository commentRepository,
            ITimeLogRepository timeLogRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<TaskService> logger)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _commentRepository = commentRepository;
            _timeLogRepository = timeLogRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<(IEnumerable<TaskListViewModel> Tasks, string ProjectName)?> GetTasksByProjectAsync(int projectId)
        {
            try
            {
                _logger.LogInformation("Fetching tasks for project {ProjectId}", projectId);

                var project = await _projectRepository.GetByIdAsync(projectId);
                if (project == null)
                {
                    _logger.LogWarning("Project {ProjectId} not found", projectId);
                    return null;
                }

                var tasks = await _taskRepository.GetTasksByProjectAsync(projectId);

                var viewModels = tasks.Select(t => new TaskListViewModel
                {
                    Id = t.Id,
                    Tag = t.Tag,
                    Title = t.Title,
                    Type = t.Type,
                    Priority = t.Priority,
                    Status = t.Status,
                    Deadline = t.Deadline,
                    AssigneeName = UserDisplayNameHelper.GetFullName(t.Assignee)
                }).ToList();

                _logger.LogInformation("Fetched {Count} tasks for project {ProjectId}", viewModels.Count, projectId);
                return (viewModels, project.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tasks for project {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<TaskViewModel> GetTaskViewModelForCreateAsync()
        {
            try
            {
                _logger.LogInformation("Creating task view model for create");

                return new TaskViewModel
                {
                    Users = await GetUserSelectListAsync()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task view model");
                throw;
            }
        }

        public async Task<EditTaskViewModel?> GetTaskForEditAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching task {TaskId} for edit", id);

                var task = await _taskRepository.GetByIdAsync(id);
                if (task == null)
                {
                    _logger.LogWarning("Task {TaskId} not found", id);
                    return null;
                }

                return new EditTaskViewModel
                {
                    Title = task.Title,
                    Description = task.Description,
                    Type = task.Type,
                    Priority = task.Priority,
                    Status = task.Status,
                    Deadline = task.Deadline,
                    AssigneeId = task.AssigneeId,
                    Users = await GetUserSelectListAsync()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task {TaskId} for edit", id);
                throw;
            }
        }

        public async Task<TaskDetailsViewModel?> GetTaskForDeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching task {TaskId} for delete", id);

                var task = await _taskRepository.GetByIdAsync(id);
                if (task == null)
                {
                    _logger.LogWarning("Task {TaskId} not found", id);
                    return null;
                }

                return new TaskDetailsViewModel
                {
                    Id = task.Id,
                    Tag = task.Tag,
                    Title = task.Title,
                    Description = task.Description,
                    Type = task.Type,
                    Priority = task.Priority,
                    Status = task.Status,
                    Deadline = task.Deadline,
                    AssigneeName = UserDisplayNameHelper.GetFullName(task.Assignee)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task {TaskId} for delete", id);
                throw;
            }
        }

        public async Task<TaskDetailsViewModel?> GetTaskDetailsAsync(int projectId, int id, string currentUserId)
        {
            try
            {
                _logger.LogInformation("Fetching task {TaskId} details for project {ProjectId}", id, projectId);

                var task = await _taskRepository.GetByIdAsync(id);
                if (task == null)
                {
                    _logger.LogWarning("Task {TaskId} not found", id);
                    return null;
                }

                var comments = await _commentRepository.GetCommentsByTaskAsync(id);
                var timeLogs = await _timeLogRepository.GetTimeLogsByTaskAsync(id);

                return new TaskDetailsViewModel
                {
                    Id = task.Id,
                    Tag = task.Tag,
                    Title = task.Title,
                    Description = task.Description,
                    Type = task.Type,
                    Priority = task.Priority,
                    Status = task.Status,
                    Deadline = task.Deadline,
                    AssigneeName = UserDisplayNameHelper.GetFullName(task.Assignee),
                    ReporterName = UserDisplayNameHelper.GetFullName(task.Reporter),
                    ProjectId = projectId,
                    Comments = comments.Select(c => new CommentListViewModel
                    {
                        Id = c.Id,
                        Content = c.Content,
                        AuthorName = UserDisplayNameHelper.GetFullName(c.User),
                        CreatedAt = c.CreatedAt,
                        CanEdit = c.UserId == currentUserId
                    }),
                    TimeLogs = timeLogs.Select(t => new TimeLogListViewModel
                    {
                        Id = t.Id,
                        Hours = t.Hours,
                        Date = t.Date,
                        Description = t.Description,
                        UserName = UserDisplayNameHelper.GetFullName(t.User),
                        CanEdit = t.UserId == currentUserId
                    }),
                    TotalHours = timeLogs.Sum(t => t.Hours)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching task {TaskId} details", id);
                throw;
            }
        }

        public async Task<(IEnumerable<TaskListViewModel> Tasks, string ProjectName, string ProjectTag)?> GetTasksForBoardAsync(int projectId)
        {
            try
            {
                _logger.LogInformation("Fetching tasks for board view, project {ProjectId}", projectId);

                var project = await _projectRepository.GetByIdAsync(projectId);
                if (project == null)
                {
                    _logger.LogWarning("Project {ProjectId} not found for board view", projectId);
                    return null;
                }

                var tasks = await _taskRepository.GetTasksByProjectAsync(projectId);

                var viewModels = tasks.Select(t => new TaskListViewModel
                {
                    Id = t.Id,
                    Tag = t.Tag,
                    Title = t.Title,
                    Type = t.Type,
                    Priority = t.Priority,
                    Status = t.Status,
                    Deadline = t.Deadline,
                    AssigneeName = UserDisplayNameHelper.GetFullName(t.Assignee)
                }).ToList();

                return (viewModels, project.Name, project.Tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching board view for project {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<bool> CreateTaskAsync(int projectId, TaskViewModel model, string currentUserId)
        {
            try
            {
                _logger.LogInformation("Creating task {TaskTitle} for project {ProjectId}", model.Title, projectId);

                var task = new ProjectTask
                {
                    Title = model.Title,
                    Description = model.Description,
                    Type = model.Type,
                    Priority = model.Priority,
                    Status = ProjectTaskStatus.ToDo,
                    Deadline = model.Deadline,
                    ProjectId = projectId,
                    AssigneeId = model.AssigneeId,
                    ReporterId = currentUserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _taskRepository.AddAsync(task);

                _logger.LogInformation("Task {TaskTitle} created successfully with ID {TaskId}", model.Title, task.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task {TaskTitle} for project {ProjectId}", model.Title, projectId);
                throw;
            }
        }

        public async Task<bool> UpdateTaskAsync(int id, EditTaskViewModel model)
        {
            try
            {
                _logger.LogInformation("Updating task {TaskId}", id);

                var dto = new UpdateTaskDto
                {
                    Title = model.Title,
                    Description = model.Description,
                    Type = model.Type,
                    Priority = model.Priority,
                    Status = model.Status,
                    Deadline = model.Deadline,
                    AssigneeId = model.AssigneeId
                };

                var updated = await _taskRepository.UpdateTaskAsync(id, dto);

                if (updated)
                {
                    _logger.LogInformation("Task {TaskId} updated successfully", id);
                }
                else
                {
                    _logger.LogWarning("Task {TaskId} not found for update", id);
                }

                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting task {TaskId}", id);

                var deleted = await _taskRepository.DeleteAsync(id);

                if (deleted)
                {
                    _logger.LogInformation("Task {TaskId} deleted successfully", id);
                }
                else
                {
                    _logger.LogWarning("Task {TaskId} not found for deletion", id);
                }

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateTaskStatusAsync(int id, ProjectTaskStatus status)
        {
            try
            {
                _logger.LogInformation("Updating task {TaskId} status to {Status}", id, status);

                var updated = await _taskRepository.UpdateStatusAsync(id, status);

                if (updated)
                {
                    _logger.LogInformation("Task {TaskId} status updated successfully to {Status}", id, status);
                }
                else
                {
                    _logger.LogWarning("Task {TaskId} not found for status update", id);
                }

                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId} status", id);
                throw;
            }
        }

        private async Task<List<SelectListItem>> GetUserSelectListAsync()
        {
            return await _userManager.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = UserDisplayNameHelper.GetFullName(u)
                }).ToListAsync();
        }
    }
}