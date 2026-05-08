namespace ProjectManagementSystem.Services
{
    using Constants;
    using DTOs;
    using Enums;
    using Enums.Task;
    using Interfaces;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Models;
    using ViewModels.Comments;
    using ViewModels.Tasks;
    using ViewModels.TimeLogs;

    public class TaskService(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        ICommentRepository commentRepository,
        ITimeLogRepository timeLogRepository,
        IActivityService activityService,
        UserManager<ApplicationUser> userManager,
        ILogger<TaskService> logger) : ITaskService
    {
        private readonly ITaskRepository _taskRepository = taskRepository;
        private readonly IProjectRepository _projectRepository = projectRepository;
        private readonly ICommentRepository _commentRepository = commentRepository;
        private readonly ITimeLogRepository _timeLogRepository = timeLogRepository;
        private readonly IActivityService _activityService = activityService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly ILogger<TaskService> _logger = logger;

        public async Task<(IEnumerable<TaskListViewModel> Tasks, string ProjectName)?> GetTasksByProjectAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return null;

            var tasks = await _taskRepository.GetTasksByProjectAsync(projectId);

            var viewModels = tasks.Select(t => new TaskListViewModel
            {
                Id = t.Id,
                Tag = t.Tag ?? MessageConstants.UntitledTask,
                Title = t.Title,
                Type = t.Type,
                Priority = t.Priority,
                Status = t.Status,
                Deadline = t.Deadline,
                AssigneeName = t.Assignee?.FullName ?? MessageConstants.Unassigned
            }).ToList();

            return (viewModels, project.Name ?? MessageConstants.UntitledProject);
        }

        public async Task<TaskViewModel> GetTaskViewModelForCreateAsync()
        {
            return new TaskViewModel
            {
                Title = string.Empty,
                Type = Type.Task,
                Priority = Priority.Low,
                Users = await GetUserSelectListAsync()
            };
        }

        public async Task<EditTaskViewModel?> GetTaskForEditAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return null;

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

        public async Task<TaskDetailsViewModel?> GetTaskForDeleteAsync(int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return null;

            return new TaskDetailsViewModel
            {
                Id = task.Id,
                Tag = task.Tag ?? MessageConstants.UntitledTask,
                Title = task.Title,
                Description = task.Description,
                Type = task.Type,
                Priority = task.Priority,
                Status = task.Status,
                Deadline = task.Deadline,
                AssigneeName = task.Assignee?.FullName ?? MessageConstants.Unassigned,
                ReporterName = task.Reporter?.FullName ?? MessageConstants.SystemUser
            };
        }

        public async Task<TaskDetailsViewModel?> GetTaskDetailsAsync(int projectId, int id, string currentUserId)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return null;

            var comments = await _commentRepository.GetCommentsByTaskAsync(id);
            var timeLogs = await _timeLogRepository.GetTimeLogsByTaskAsync(id);

            return new TaskDetailsViewModel
            {
                Id = task.Id,
                Tag = task.Tag ?? MessageConstants.UntitledTask,
                Title = task.Title,
                Description = task.Description,
                Type = task.Type,
                Priority = task.Priority,
                Status = task.Status,
                Deadline = task.Deadline,
                AssigneeName = task.Assignee?.FullName ?? MessageConstants.Unassigned,
                ReporterName = task.Reporter.FullName,
                ProjectId = projectId,
                Comments = comments.Select(c => new CommentListViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    AuthorName = c.User.FullName,
                    CreatedAt = c.CreatedAt,
                    CanEdit = c.UserId == currentUserId
                }).ToList(),
                TimeLogs = timeLogs.Select(t => new TimeLogListViewModel
                {
                    Id = t.Id,
                    Hours = t.Hours,
                    Date = t.Date,
                    Description = t.Description,
                    UserName = t.User.FullName,
                    CanEdit = t.UserId == currentUserId
                }).ToList(),
                TotalHours = timeLogs.Sum(t => t.Hours)
            };
        }

        public async Task<(IEnumerable<TaskListViewModel> Tasks, string ProjectName, string ProjectTag)?> GetTasksForBoardAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return null;

            var tasks = await _taskRepository.GetTasksByProjectAsync(projectId);

            var viewModels = tasks.Select(t => new TaskListViewModel
            {
                Id = t.Id,
                Tag = t.Tag ?? MessageConstants.UntitledTask,
                Title = t.Title,
                Type = t.Type,
                Priority = t.Priority,
                Status = t.Status,
                Deadline = t.Deadline,
                AssigneeName = t.Assignee?.FullName ?? MessageConstants.Unassigned
            }).ToList();

            return (viewModels, project.Name ?? MessageConstants.UntitledProject, project.Tag ?? MessageConstants.UntitledProject);
        }

        public async Task<bool> CreateTaskAsync(int projectId, TaskViewModel model, string currentUserId)
        {
            var task = new ProjectTask
            {
                Title = model.Title,
                Description = model.Description,
                Type = model.Type,
                Priority = model.Priority,
                Status = Status.ToDo,
                Deadline = model.Deadline,
                ProjectId = projectId,
                AssigneeId = model.AssigneeId,
                ReporterId = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _taskRepository.AddAsync(task);

            await _activityService.LogAsync(
                currentUserId,
                string.Format(MessageConstants.ActivityCreatedTask, model.Title),
                ActivityType.TaskAction);

            _logger.LogInformation("Task {TaskTitle} created by User {UserId} in Project {ProjectId}", model.Title, currentUserId, projectId);
            return true;
        }

        public async Task<bool> UpdateTaskAsync(int id, EditTaskViewModel model, string userId)
        {
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
                await _activityService.LogAsync(
                    userId,
                    string.Format(MessageConstants.ActivityUpdatedTask, model.Title),
                    ActivityType.TaskAction);

                _logger.LogInformation("Task {TaskId} updated by User {UserId}", id, userId);
            }
            return updated;
        }

        public async Task<bool> DeleteTaskAsync(int id, string userId)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            var taskTitle = task?.Title ?? MessageConstants.MissingTaskIdentifier;

            var deleted = await _taskRepository.DeleteAsync(id);
            if (deleted)
            {
                await _activityService.LogAsync(
                    userId,
                    string.Format(MessageConstants.ActivityDeletedTask, taskTitle),
                    ActivityType.TaskAction);

                _logger.LogInformation("Task {TaskId} deleted by User {UserId}", id, userId);
            }
            return deleted;
        }

        public async Task<bool> UpdateTaskStatusAsync(int id, Status status, string userId)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            var taskTag = task?.Tag ?? MessageConstants.UntitledTask;

            var updated = await _taskRepository.UpdateStatusAsync(id, status);
            if (updated)
            {
                await _activityService.LogAsync(
                    userId,
                    string.Format(MessageConstants.ActivityMovedTask, taskTag, status),
                    ActivityType.TaskAction);

                _logger.LogInformation("Task {TaskId} status updated to {Status} by User {UserId}", id, status, userId);
            }
            return updated;
        }

        private async Task<List<SelectListItem>> GetUserSelectListAsync()
        {
            return await _userManager.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FullName
                }).ToListAsync();
        }
    }
}