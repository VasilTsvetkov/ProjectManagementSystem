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
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return null;

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

            return (viewModels, project.Name);
        }

        public async Task<TaskViewModel> GetTaskViewModelForCreateAsync()
        {
            return new TaskViewModel
            {
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

        public async Task<TaskDetailsViewModel?> GetTaskDetailsAsync(int projectId, int id, string currentUserId)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return null;

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

        public async Task<(IEnumerable<TaskListViewModel> Tasks, string ProjectName, string ProjectTag)?> GetTasksForBoardAsync(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return null;

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

        public async Task<bool> CreateTaskAsync(int projectId, TaskViewModel model, string currentUserId)
        {
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
            _logger.LogInformation("Task {TaskTitle} created by User {UserId}", model.Title, currentUserId);
            return true;
        }

        public async Task<bool> UpdateTaskAsync(int id, EditTaskViewModel model)
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
            if (updated) _logger.LogInformation("Task {TaskId} updated", id);
            return updated;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var deleted = await _taskRepository.DeleteAsync(id);
            if (deleted) _logger.LogInformation("Task {TaskId} deleted", id);
            return deleted;
        }

        public async Task<bool> UpdateTaskStatusAsync(int id, ProjectTaskStatus status)
        {
            var updated = await _taskRepository.UpdateStatusAsync(id, status);
            if (updated) _logger.LogInformation("Task {TaskId} status updated to {Status}", id, status);
            return updated;
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