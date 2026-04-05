namespace ProjectManagementSystem.Controllers
{
    using DTOs;
    using Enums;
    using Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Models;
    using ViewModels.Comments;
    using ViewModels.Tasks;
    using ViewModels.TimeLogs;

    [Authorize]
    [Route("tasks")]
    public class TasksController : Controller
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICommentRepository _commentRepository;
        private readonly ITimeLogRepository _timeLogRepository;

        public TasksController(ITaskRepository taskRepository, IProjectRepository projectRepository, ICommentRepository commentRepository, ITimeLogRepository timeLogRepository, UserManager<ApplicationUser> userManager)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _commentRepository = commentRepository;
            _timeLogRepository = timeLogRepository;
            _userManager = userManager;
        }

        [HttpGet("{projectId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Index(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return NotFound();

            var tasks = await _taskRepository.GetTasksByProjectAsync(projectId);

            var model = tasks.Select(t => new TaskListViewModel
            {
                Id = t.Id,
                Tag = t.Tag,
                Title = t.Title,
                Type = t.Type,
                Priority = t.Priority,
                Status = t.Status,
                Deadline = t.Deadline,
                AssigneeEmail = t.Assignee?.Email,
                AssigneeName = GetAssigneeName(t.Assignee)
            });

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = project.Name;

            return View(model);
        }

        [HttpGet("{projectId}/create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Create(int projectId)
        {
            var model = new TaskViewModel
            {
                Users = GetUserSelectList()
            };
            return View(model);
        }

        [HttpPost("{projectId}/create")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create(int projectId, TaskViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = GetUserSelectList();
                return View(model);
            }

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
                CreatedAt = DateTime.UtcNow
            };

            await _taskRepository.AddAsync(task);
            return RedirectToAction(nameof(Index), new { projectId });
        }

        [HttpGet("{projectId}/{id}/edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(int projectId, int id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return NotFound();

            var model = new EditTaskViewModel
            {
                Title = task.Title,
                Description = task.Description,
                Type = task.Type,
                Priority = task.Priority,
                Status = task.Status,
                Deadline = task.Deadline,
                AssigneeId = task.AssigneeId,
                Users = GetUserSelectList()
            };

            return View(model);
        }

        [HttpPost("{projectId}/{id}/edit")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Edit(int projectId, int id, EditTaskViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Users = GetUserSelectList();
                return View(model);
            }

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
            if (!updated) return NotFound();

            return RedirectToAction(nameof(Index), new { projectId });
        }

        [HttpGet("{projectId}/{id}/delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int projectId, int id)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            var model = new TaskDetailsViewModel
            {
                Id = task.Id,
                Tag = task.Tag,
                Title = task.Title,
                Description = task.Description,
                Type = task.Type,
                Priority = task.Priority,
                Status = task.Status,
                Deadline = task.Deadline,
                AssigneeEmail = task.Assignee?.Email,
                AssigneeName = GetAssigneeName(task.Assignee)
            };

            return View(model);
        }

        [HttpPost("{projectId}/{id}/delete")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteConfirmed(int projectId, int id)
        {
            var deleted = await _taskRepository.DeleteAsync(id);
            if (!deleted) return NotFound();
            return RedirectToAction(nameof(Index), new { projectId });
        }

        [HttpGet("{projectId}/{id}/details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Details(int projectId, int id)
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            var comments = await _commentRepository.GetCommentsByTaskAsync(id);
            var timeLogs = await _timeLogRepository.GetTimeLogsByTaskAsync(id);
            var userId = _userManager.GetUserId(User);

            var model = new TaskDetailsViewModel
            {
                Id = task.Id,
                Tag = task.Tag,
                Title = task.Title,
                Description = task.Description,
                Type = task.Type,
                Priority = task.Priority,
                Status = task.Status,
                Deadline = task.Deadline,
                AssigneeEmail = task.Assignee?.Email,
                AssigneeName = GetAssigneeName(task.Assignee),
                ProjectId = projectId,
                Comments = comments.Select(c => new CommentListViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    AuthorEmail = c.User?.Email,
                    CreatedAt = c.CreatedAt,
                    CanEdit = c.UserId == userId
                }),
                TimeLogs = timeLogs.Select(t => new TimeLogListViewModel
                {
                    Id = t.Id,
                    Hours = t.Hours,
                    Date = t.Date,
                    Description = t.Description,
                    UserEmail = t.User?.Email,
                    CanEdit = t.UserId == userId
                }),
                TotalHours = timeLogs.Sum(t => t.Hours)
            };

            return View(model);
        }

        [HttpPost("{projectId}/{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int projectId, int id, [FromBody] ProjectTaskStatus status)
        {
            var updated = await _taskRepository.UpdateStatusAsync(id, status);
            if (!updated) return NotFound();
            return Ok();
        }

        [HttpGet("{projectId}/board")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Board(int projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return NotFound();

            var tasks = await _taskRepository.GetTasksByProjectAsync(projectId);

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectTag = project.Tag;

            var model = tasks.Select(t => new TaskListViewModel
            {
                Id = t.Id,
                Tag = t.Tag,
                Title = t.Title,
                Type = t.Type,
                Priority = t.Priority,
                Status = t.Status,
                Deadline = t.Deadline,
                AssigneeEmail = t.Assignee?.Email,
                AssigneeName = GetAssigneeName(t.Assignee)
            });

            return View(model);
        }

        private List<SelectListItem> GetUserSelectList()
            => _userManager.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FirstName} {u.LastName}"
                }).ToList();

        private string? GetAssigneeName(ApplicationUser? assignee)
            => assignee != null ? $"{assignee.FirstName} {assignee.LastName}" : null;
    }
}