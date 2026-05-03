namespace ProjectManagementSystem.Controllers
{
    using Constants;
    using Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using ViewModels.TimeLogs;

    [Authorize]
    [Route("timelogs")]
    public class TimeLogsController(ITimeLogService timeLogService, UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly ITimeLogService _timeLogService = timeLogService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        [HttpGet("matrix/{projectId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Matrix(int projectId, int? month, int? year)
        {
            int selectedMonth = month ?? DateTime.UtcNow.Month;
            int selectedYear = year ?? DateTime.UtcNow.Year;

            var viewModel = await _timeLogService.GetMonthlyMatrixAsync(projectId, selectedMonth, selectedYear);

            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
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

            var success = await _timeLogService.CreateTimeLogAsync(model, userId);

            if (!success)
            {
                TempData["Error"] = $"Daily limit reached. You cannot log more than {TimeConfig.WorkingHoursPerDay} hours per day.";
            }

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