namespace ProjectManagementSystem.Controllers
{
    using Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using ViewModels.TimeLogs;

    [Authorize]
    [Route("timelogs")]
    public class TimeLogsController : Controller
    {
        private readonly ITimeLogService _timeLogService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TimeLogsController(ITimeLogService timeLogService, UserManager<ApplicationUser> userManager)
        {
            _timeLogService = timeLogService;
            _userManager = userManager;
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(TimeLogViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Details", "Tasks", new { projectId = model.ProjectId, id = model.TaskId });

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized();
            }

            await _timeLogService.CreateTimeLogAsync(model, userId);

            return RedirectToAction("Details", "Tasks", new { projectId = model.ProjectId, id = model.TaskId });
        }

        [HttpPost("{id}/delete")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int id, int projectId)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _timeLogService.DeleteTimeLogAsync(id, userId);

            if (result == null) return NotFound();

            return RedirectToAction("Details", "Tasks", new { projectId, id = result.Value.TaskId });
        }
    }
}