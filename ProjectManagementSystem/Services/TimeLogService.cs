namespace ProjectManagementSystem.Services
{
    using Constants;
    using Enums;
    using Helpers;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Models;
    using ViewModels.TimeLogs;

    public class TimeLogService(
        ITimeLogRepository timeLogRepository,
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IActivityService activityService,
        ILogger<TimeLogService> logger) : ITimeLogService
    {
        private readonly ITimeLogRepository _timeLogRepository = timeLogRepository;
        private readonly ITaskRepository _taskRepository = taskRepository;
        private readonly IProjectRepository _projectRepository = projectRepository;
        private readonly IActivityService _activityService = activityService;
        private readonly ILogger<TimeLogService> _logger = logger;

        public async Task<bool> CreateTimeLogAsync(TimeLogViewModel model, string userId)
        {
            var newHours = ((double)model.Days * TimeConfig.WorkingHoursPerDay) + model.Hours + (model.Minutes / TimeConfig.MinutesInHour);

            var existingLogs = await _timeLogRepository.GetLogsByProjectAndDateAsync(model.ProjectId, model.Date, model.Date);
            var userExistingHours = existingLogs.Where(l => l.UserId == userId).Sum(l => l.Hours);

            if (userExistingHours + newHours > TimeConfig.WorkingHoursPerDay)
            {
                _logger.LogWarning("User {UserId} attempted to log {Total} hours on {Date}, exceeding daily limit.", userId, userExistingHours + newHours, model.Date.ToShortDateString());
                return false;
            }

            var timeLog = new TimeLog
            {
                Hours = newHours,
                Date = model.Date,
                Description = model.Description,
                TaskId = model.TaskId,
                UserId = userId
            };

            await _timeLogRepository.AddAsync(timeLog);

            var task = await _taskRepository.GetByIdAsync(model.TaskId);
            string taskTag = task?.Tag ?? "Task";

            string formattedTime = TimeFormatter.Format(newHours);
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

        public async Task<MonthlyMatrixViewModel?> GetMonthlyMatrixAsync(int projectId, int month, int year)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return null;

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var daysCount = DateTime.DaysInMonth(year, month);

            var logs = await _timeLogRepository.GetLogsByProjectAndDateAsync(projectId, startDate, endDate);

            var model = new MonthlyMatrixViewModel
            {
                ProjectId = projectId,
                ProjectName = project.Name,
                SelectedMonth = startDate,
                DaysInMonth = Enumerable.Range(0, daysCount)
                                        .Select(offset => startDate.AddDays(offset))
                                        .ToList()
            };

            var userGroups = logs.GroupBy(l => l.UserId);

            foreach (var group in userGroups)
            {
                var firstLog = group.First();
                var row = new UserMatrixRowViewModel
                {
                    UserId = group.Key,
                    FullName = $"{firstLog.User.FirstName} {firstLog.User.LastName}"
                };

                for (int i = 1; i <= daysCount; i++)
                {
                    row.DailyHours[i] = group.Where(l => l.Date.Day == i).Sum(l => l.Hours);
                }

                model.Rows.Add(row);
            }

            return model;
        }
    }
}