namespace ProjectManagementSystem.Services
{
    using Interfaces;
    using Models;
    using ViewModels.TimeLogs;
    using Microsoft.Extensions.Logging;

    public class TimeLogService : ITimeLogService
    {
        private readonly ITimeLogRepository _timeLogRepository;
        private readonly ILogger<TimeLogService> _logger;

        public TimeLogService(ITimeLogRepository timeLogRepository, ILogger<TimeLogService> logger)
        {
            _timeLogRepository = timeLogRepository;
            _logger = logger;
        }

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
            var deleted = await _timeLogRepository.DeleteAsync(id);

            if (deleted)
            {
                _logger.LogInformation("Time log {TimeLogId} deleted by User {UserId}", id, userId);
                return (true, taskId);
            }

            return null;
        }
    }
}