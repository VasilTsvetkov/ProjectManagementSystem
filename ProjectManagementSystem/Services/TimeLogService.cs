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
                _logger.LogWarning("User {UserId} attempted to log {TotalHours} hours on {Date}, exceeding daily limit.", userId, userExistingHours + newHours, model.Date.ToShortDateString());
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

            string taskTag = await GetTaskTagAsync(model.TaskId);
            string formattedTime = TimeFormatter.Format(newHours);

            await _activityService.LogAsync(
                userId,
                string.Format(MessageConstants.ActivityLoggedTime, formattedTime, taskTag),
                ActivityType.TimeLogAction);

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
            string taskTag = await GetTaskTagAsync(taskId);
            string formattedTime = TimeFormatter.Format(timeLog.Hours);

            var deleted = await _timeLogRepository.DeleteAsync(id);

            if (deleted)
            {
                await _activityService.LogAsync(
                    userId,
                    string.Format(MessageConstants.ActivityDeletedTimeLog, formattedTime, taskTag),
                    ActivityType.TimeLogAction);

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
                ProjectName = project.Name ?? MessageConstants.UntitledProject,
                SelectedMonth = startDate,
                DaysInMonth = Enumerable.Range(0, daysCount)
                                        .Select(offset => startDate.AddDays(offset))
                                        .ToList()
            };

            var userGroups = logs.GroupBy(l => l.UserId);
            var rowsList = new List<UserMatrixRowViewModel>();

            foreach (var group in userGroups)
            {
                var firstLog = group.First();
                var workingHours = new Dictionary<int, double>();

                for (int i = 1; i <= daysCount; i++)
                {
                    var sum = group.Where(l => l.Date.Day == i).Sum(l => l.Hours);
                    if (sum > 0)
                    {
                        workingHours[i] = sum;
                    }
                }

                var row = new UserMatrixRowViewModel
                {
                    UserId = group.Key,
                    FullName = firstLog.User.FullName,
                    DailyHours = workingHours
                };

                rowsList.Add(row);
            }

            return model;
        }

        private async Task<string> GetTaskTagAsync(int taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null) return MessageConstants.MissingTaskIdentifier;

            return !string.IsNullOrWhiteSpace(task.Tag) ? task.Tag : MessageConstants.UntitledTask;
        }
    }
}