namespace ProjectManagementSystem.Web.Controllers
{
    using BL.Constants;
    using BL.DTOs.TimeLogs;
    using BL.Interfaces;
    using BL.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
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

            var matrixDto = await _timeLogService.GetMonthlyMatrixAsync(projectId, selectedMonth, selectedYear);

            if (matrixDto == null)
            {
                return NotFound();
            }

            var viewModel = new MonthlyMatrixViewModel
            {
                ProjectId = matrixDto.ProjectId,
                ProjectName = matrixDto.ProjectName,
                SelectedMonth = matrixDto.SelectedMonth,
                DaysInMonth = matrixDto.DaysInMonth,
                Rows = matrixDto.Rows.Select(row => new UserMatrixRowViewModel
                {
                    UserId = row.UserId,
                    FullName = row.FullName,
                    DailyHours = row.DailyHours
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(TimeLogViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(TaskConstants.DetailsAction, TaskConstants.Controller, new { projectId = model.ProjectId, id = model.TaskId });

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized();
            }

            if (model.Date.Date > DateTime.UtcNow.Date)
            {
                TempData[NotificationKeys.Error] = "You cannot log time for future dates.";
                return RedirectToAction(TaskConstants.DetailsAction, TaskConstants.Controller, new { projectId = model.ProjectId, id = model.TaskId });
            }

            if (model.Hours == 0 && model.Minutes == 0 && model.Days == 0)
            {
                TempData[NotificationKeys.Error] = "Please enter the amount of time worked.";
                return RedirectToAction(TaskConstants.DetailsAction, TaskConstants.Controller, new { projectId = model.ProjectId, id = model.TaskId });
            }

            var dto = new TimeLogDto
            {
                Days = model.Days,
                Hours = model.Hours,
                Minutes = model.Minutes,
                Date = model.Date,
                Description = model.Description,
                TaskId = model.TaskId,
                ProjectId = model.ProjectId
            };

            var success = await _timeLogService.CreateTimeLogAsync(dto, userId);

            if (!success)
            {
                TempData[NotificationKeys.Error] = $"Daily limit reached. You cannot log more than {TimeConfig.WorkingHoursPerDay} hours per day.";
            }

            return RedirectToAction(TaskConstants.DetailsAction, TaskConstants.Controller, new { projectId = model.ProjectId, id = model.TaskId });
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

            return RedirectToAction(TaskConstants.DetailsAction, TaskConstants.Controller, new { projectId, id = result.Value.TaskId });
        }
    }
}