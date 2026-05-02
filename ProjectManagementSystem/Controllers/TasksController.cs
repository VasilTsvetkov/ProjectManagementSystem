namespace ProjectManagementSystem.Controllers
{
    using Constants;
    using Enums;
    using Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using System.Security.Claims;
    using ViewModels.Tasks;

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

            return View(result.Value.Tasks);
        }

        [HttpGet("{projectId}/create")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create(int projectId)
        {
            var model = await _taskService.GetTaskViewModelForCreateAsync();
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
                var createModel = await _taskService.GetTaskViewModelForCreateAsync();
                model.Users = createModel.Users;
                return View(model);
            }

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == null) return Unauthorized();

            await _taskService.CreateTaskAsync(projectId, model, currentUserId);
            return RedirectToAction(nameof(Index), new { projectId });
        }

        [HttpGet("{projectId}/{id}/edit")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(int projectId, int id)
        {
            var model = await _taskService.GetTaskForEditAsync(id);
            if (model == null) return NotFound();

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
                var editModel = await _taskService.GetTaskForEditAsync(id);
                if (editModel != null)
                {
                    model.Users = editModel.Users;
                }
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var updated = await _taskService.UpdateTaskAsync(id, model, userId);
            if (!updated) return NotFound();

            return RedirectToAction(nameof(Index), new { projectId });
        }

        [HttpGet("{projectId}/{id}/delete")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int projectId, int id)
        {
            var model = await _taskService.GetTaskForDeleteAsync(id);
            if (model == null) return NotFound();

            ViewBag.ProjectId = projectId;
            return View(model);
        }

        [HttpPost("{projectId}/{id}/delete")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteConfirmed(int projectId, int id)
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

            var model = await _taskService.GetTaskDetailsAsync(projectId, id, userId);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost("{projectId}/{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(int projectId, int id, [FromBody] ProjectTaskStatus status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var updated = await _taskService.UpdateTaskStatusAsync(id, status, userId);
            if (!updated || projectId == 0) return NotFound();

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

            return View(result.Value.Tasks);
        }
    }
}