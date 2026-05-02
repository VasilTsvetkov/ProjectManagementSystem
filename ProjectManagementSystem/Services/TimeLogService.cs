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
            try
            {
                _logger.LogInformation("Creating time log for task {TaskId} by user {UserId}", model.TaskId, userId);

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
                _logger.LogInformation("Time log created successfully for task {TaskId}", model.TaskId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating time log for task {TaskId}", model.TaskId);
                throw;
            }
        }

        public async Task<(bool Success, int TaskId)?> DeleteTimeLogAsync(int id, string userId)
        {
            try
            {
                _logger.LogInformation("Deleting time log {TimeLogId} by user {UserId}", id, userId);

                var timeLog = await _timeLogRepository.GetByIdAsync(id);
                if (timeLog == null)
                {
                    _logger.LogWarning("Time log {TimeLogId} not found", id);
                    return null;
                }

                if (timeLog.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} is not authorized to delete time log {TimeLogId}", userId, id);
                    return null;
                }

                var taskId = timeLog.TaskId;
                var deleted = await _timeLogRepository.DeleteAsync(id);

                if (deleted)
                {
                    _logger.LogInformation("Time log {TimeLogId} deleted successfully", id);
                    return (true, taskId);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting time log {TimeLogId}", id);
                throw;
            }
        }
    }
}