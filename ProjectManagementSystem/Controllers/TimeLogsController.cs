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
        private readonly ITimeLogRepository _timeLogRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public TimeLogsController(ITimeLogRepository timeLogRepository, UserManager<ApplicationUser> userManager)
        {
            _timeLogRepository = timeLogRepository;
            _userManager = userManager;
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(TimeLogViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Details", "Tasks", new { projectId = model.ProjectId, id = model.TaskId });

            var totalHours = model.Days * 8 + model.Hours + model.Minutes / 60.0;

            var timeLog = new TimeLog
            {
                Hours = totalHours,
                Date = model.Date,
                Description = model.Description,
                TaskId = model.TaskId,
                UserId = _userManager.GetUserId(User)
            };

            await _timeLogRepository.AddAsync(timeLog);
            return RedirectToAction("Details", "Tasks", new { projectId = model.ProjectId, id = model.TaskId });
        }

        [HttpPost("{id}/delete")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, int projectId)
        {
            var timeLog = await _timeLogRepository.GetByIdAsync(id);
            if (timeLog == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (timeLog.UserId != userId) return Forbid();

            var taskId = timeLog.TaskId;
            var deleted = await _timeLogRepository.DeleteAsync(id);
            if (!deleted) return NotFound();

            return RedirectToAction("Details", "Tasks", new { projectId, id = taskId });
        }
    }
}