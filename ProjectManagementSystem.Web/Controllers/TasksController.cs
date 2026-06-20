namespace ProjectManagementSystem.Web.Controllers
{
    using BL.Constants;
    using BL.DTOs.Tasks;
    using BL.Enums.Task;
    using BL.Interfaces;
    using BL.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using ViewModels.Comments;
    using ViewModels.Tasks;
    using ViewModels.TimeLogs;

    [Authorize]
    [Route("tasks")]
    public class TasksController(ITaskService taskService, UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly ITaskService _taskService = taskService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        [HttpGet("{projectId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Index(int projectId)
        {
            var result = await _taskService.GetTasksByProjectAsync(projectId);
            if (result == null) return NotFound();

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = result.Value.ProjectName;

            var viewModels = result.Value.Tasks.Select(dto => new TaskListViewModel
            {
                Id = dto.Id,
                ProjectId = projectId,
                Tag = dto.Tag,
                Title = dto.Title,
                Type = dto.Type,
                Priority = dto.Priority,
                Status = dto.Status,
                Deadline = dto.Deadline,
                AssigneeName = dto.AssigneeName
            }).ToList();

            return View(viewModels);
        }

        [HttpGet("{projectId}/create")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create(int projectId)
        {
            var dto = await _taskService.GetTaskForCreateAsync();

            var model = new TaskViewModel
            {
                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type,
                Priority = dto.Priority,
                Deadline = dto.Deadline,
                AssigneeId = dto.AssigneeId,
                Users = dto.Users.ConvertAll(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FullName
                })
            };

            ViewBag.ProjectId = projectId;
            return View(model);
        }

        [HttpPost("{projectId}/create")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create(int projectId, TaskViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var createDto = await _taskService.GetTaskForCreateAsync();
                model.Users = createDto.Users.ConvertAll(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FullName
                });
                return View(model);
            }

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            var dto = new TaskDto
            {
                Title = model.Title,
                Description = model.Description,
                AssigneeId = model.AssigneeId,
                Priority = model.Priority,
                Type = model.Type,
                Deadline = model.Deadline
            };

            await _taskService.CreateTaskAsync(projectId, dto, currentUserId);
            return RedirectToAction(nameof(Index), new { projectId });
        }

        [HttpGet("{projectId}/{id}/edit")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(int projectId, int id)
        {
            var dto = await _taskService.GetTaskForEditAsync(id);
            if (dto == null) return NotFound();

            var model = new EditTaskViewModel
            {
                Title = dto.Title,
                Description = dto.Description,
                AssigneeId = dto.AssigneeId,
                Priority = dto.Priority,
                Type = dto.Type,
                Deadline = dto.Deadline,
                Status = dto.Status,
                Users = dto.Users.ConvertAll(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.FullName,
                    Selected = u.Id == dto.AssigneeId
                })
            };

            ViewBag.ProjectId = projectId;
            return View(model);
        }

        [HttpPost("{projectId}/{id}/edit")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Edit(int projectId, int id, EditTaskViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var editDto = await _taskService.GetTaskForEditAsync(id);
                if (editDto != null)
                {
                    model.Users = editDto.Users.ConvertAll(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.FullName
                    });
                }
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var dto = new TaskDto
            {
                Id = id,
                Title = model.Title,
                Description = model.Description,
                AssigneeId = model.AssigneeId,
                Priority = model.Priority,
                Type = model.Type,
                Deadline = model.Deadline,
                Status = model.Status
            };

            var updated = await _taskService.UpdateTaskAsync(id, dto, userId);
            if (!updated) return NotFound();

            return RedirectToAction(nameof(Index), new { projectId });
        }

        [HttpPost("{projectId}/{id}/delete")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int projectId, int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var deleted = await _taskService.DeleteTaskAsync(id, userId);
            if (!deleted) return NotFound();

            return RedirectToAction(nameof(Index), new { projectId });
        }

        [HttpGet("{projectId}/{id}/details")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Details(int projectId, int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var dto = await _taskService.GetTaskDetailsAsync(projectId, id, userId);
            if (dto == null) return NotFound();

            var model = new TaskDetailsViewModel
            {
                Id = dto.Id,
                ProjectId = dto.ProjectId,
                Tag = dto.Tag,
                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type,
                Priority = dto.Priority,
                Status = dto.Status,
                Deadline = dto.Deadline,
                AssigneeName = dto.AssigneeName,
                ReporterName = dto.ReporterName,
                TotalHours = dto.TotalHours,
                Comments = dto.Comments.Select(c => new CommentListViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    AuthorName = c.AuthorName,
                    CreatedAt = c.CreatedAt,
                    CanEdit = c.CanEdit
                }).ToList(),
                TimeLogs = dto.TimeLogs.Select(t => new TimeLogListViewModel
                {
                    Id = t.Id,
                    Hours = t.Hours,
                    Date = t.Date,
                    Description = t.Description,
                    UserName = t.UserName ?? MessageConstants.Unassigned,
                    CanEdit = t.CanEdit
                }).ToList()
            };

            return View(model);
        }

        [HttpPost("{projectId}/{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int projectId, int id, [FromBody] Status status)
        {
            _ = projectId;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var updated = await _taskService.UpdateTaskStatusAsync(id, status, userId);
            if (!updated) return NotFound();

            return Ok();
        }

        [HttpGet("{projectId}/board")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Board(int projectId)
        {
            var result = await _taskService.GetTasksForBoardAsync(projectId);
            if (result == null) return NotFound();

            ViewBag.ProjectId = projectId;
            ViewBag.ProjectName = result.Value.ProjectName;
            ViewBag.ProjectTag = result.Value.ProjectTag;

            var viewModels = result.Value.Tasks.Select(dto => new TaskListViewModel
            {
                Id = dto.Id,
                ProjectId = projectId,
                Tag = dto.Tag,
                Title = dto.Title,
                Type = dto.Type,
                Priority = dto.Priority,
                Status = dto.Status,
                Deadline = dto.Deadline,
                AssigneeName = dto.AssigneeName
            }).ToList();

            return View(viewModels);
        }
    }
}