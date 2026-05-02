namespace ProjectManagementSystem.Services
{
    using Interfaces;
    using Models;
    using ViewModels.TimeLogs;
    using Microsoft.Extensions.Logging;
    using Enums;
    using Helpers;

    public class TimeLogService(
        ITimeLogRepository timeLogRepository,
        ITaskRepository taskRepository,
        IActivityService activityService,
        ILogger<TimeLogService> logger) : ITimeLogService
    {
        private readonly ITimeLogRepository _timeLogRepository = timeLogRepository;
        private readonly ITaskRepository _taskRepository = taskRepository;
        private readonly IActivityService _activityService = activityService;
        private readonly ILogger<TimeLogService> _logger = logger;

        public async Task<bool> CreateTimeLogAsync(TimeLogViewModel model, string userId)
        {
            var totalHours = (double)model.Days * 8 + (double)model.Hours + (double)model.Minutes / 60.0;

            var timeLog = new TimeLog
            {
                Hours = totalHours,
                Date = model.Date,
                Description = model.Description,
                TaskId = model.TaskId,
                UserId = userId
            };

            await _timeLogRepository.AddAsync(timeLog);

            var task = await _taskRepository.GetByIdAsync(model.TaskId);
            string taskTag = task?.Tag ?? "Task";

            string formattedTime = TimeFormatter.Format(totalHours);
            await _activityService.LogAsync(userId, $"Logged {formattedTime} on {taskTag}", ActivityType.TimeLogAction);

            _logger.LogInformation("Time log created for Task {TaskId} by User {UserId}", model.TaskId, userId);

            return true;
        }

        public async Task<(bool Success, int TaskId)?> DeleteTimeLogAsync(int id, string userId)
        {
            var timeLog = await _timeLogRepository.GetByIdAsync(id);

            if (timeLog == null || timeLog.UserId != userId)
            {
                return null;
            }

            var taskId = timeLog.TaskId;
            var task = await _taskRepository.GetByIdAsync(taskId);
            string taskTag = task?.Tag ?? "Task";

            string formattedTime = TimeFormatter.Format(timeLog.Hours);

            var deleted = await _timeLogRepository.DeleteAsync(id);

            if (deleted)
            {
                await _activityService.LogAsync(userId, $"Deleted {formattedTime} log from {taskTag}", ActivityType.TimeLogAction);
                _logger.LogInformation("Time log {TimeLogId} deleted by User {UserId}", id, userId);
                return (true, taskId);
            }

            return null;
        }
    }
}