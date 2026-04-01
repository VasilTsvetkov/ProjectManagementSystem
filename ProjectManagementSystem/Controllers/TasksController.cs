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
    using ViewModels.Tasks;

    [Authorize]
    [Route("tasks")]
    public class TasksController : Controller
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public TasksController(ITaskRepository taskRepository, IProjectRepository projectRepository, UserManager<ApplicationUser> userManager)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
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
                AssigneeEmail = t.Assignee?.Email
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
                AssigneeEmail = task.Assignee?.Email
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

        private List<SelectListItem> GetUserSelectList()
            => _userManager.Users
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.Email
                }).ToList();
    }
}