namespace ProjectManagementSystem.BL.Services
{
    using Constants;
    using DTOs.TimeLogs;
    using Enums;
    using Helpers;
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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

        public async Task<bool> CreateTimeLogAsync(TimeLogDto dto, string userId)
        {
            if (dto.Date.Date > DateTime.UtcNow.Date)
            {
                _logger.LogWarning("User {UserId} attempted to log time in the future: {Date}", userId, dto.Date.ToShortDateString());
                return false;
            }

            if (dto.Hours == 0 && dto.Minutes == 0 && dto.Days == 0)
            {
                return false;
            }

            var newHours = ((double)dto.Days * TimeConfig.WorkingHoursPerDay) + dto.Hours + ((double)dto.Minutes / TimeConfig.MinutesInHour);

            var existingLogs = await _timeLogRepository.GetLogsByProjectAndDateAsync(dto.ProjectId, dto.Date, dto.Date);
            var userExistingHours = existingLogs.Where(l => l.UserId == userId).Sum(l => l.Hours);

            if (userExistingHours + newHours > TimeConfig.WorkingHoursPerDay)
            {
                _logger.LogWarning("User {UserId} attempted to log {TotalHours} hours on {Date}, exceeding daily limit.", userId, userExistingHours + newHours, dto.Date.ToShortDateString());
                return false;
            }

            var timeLog = new TimeLog
            {
                Hours = newHours,
                Date = dto.Date,
                Description = dto.Description,
                TaskId = dto.TaskId,
                UserId = userId
            };

            await _timeLogRepository.AddAsync(timeLog);

            string taskTag = await GetTaskTagAsync(dto.TaskId);
            string formattedTime = TimeFormatter.Format(newHours);

            await _activityService.LogAsync(
                userId,
                string.Format(MessageConstants.ActivityLoggedTime, formattedTime, taskTag),
                ActivityType.TimeLogAction);

            _logger.LogInformation("Time log created for Task {TaskId} by User {UserId}", dto.TaskId, userId);

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

        public async Task<MonthlyMatrixDto?> GetMonthlyMatrixAsync(int projectId, int month, int year)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return null;

            var startDate = new DateTime(year, month, 1);
            var daysCount = DateTime.DaysInMonth(year, month);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var logs = await _timeLogRepository.GetLogsByProjectAndDateAsync(projectId, startDate, endDate);

            var userGroups = logs.GroupBy(l => l.UserId);
            var rowsList = new List<UserMatrixRowDto>();

            foreach (var group in userGroups)
            {
                var firstLog = group.First();
                var workingHours = new Dictionary<int, double>();

                var dailySums = group
                    .GroupBy(l => l.Date.Day)
                    .ToDictionary(g => g.Key, g => g.Sum(l => l.Hours));

                for (int i = 1; i <= daysCount; i++)
                {
                    if (dailySums.TryGetValue(i, out double sum) && sum > 0)
                    {
                        workingHours[i] = sum;
                    }
                }

                rowsList.Add(new UserMatrixRowDto
                {
                    UserId = group.Key,
                    FullName = firstLog.User?.FullName ?? MessageConstants.SystemUser,
                    DailyHours = workingHours
                });
            }

            return new MonthlyMatrixDto
            {
                ProjectId = projectId,
                ProjectName = project.Name ?? MessageConstants.UntitledProject,
                SelectedMonth = startDate,
                DaysInMonth = Enumerable.Range(0, daysCount)
                                        .Select(offset => startDate.AddDays(offset))
                                        .ToList(),
                Rows = rowsList
            };
        }

        private async Task<string> GetTaskTagAsync(int taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null) return MessageConstants.MissingTaskIdentifier;

            return !string.IsNullOrWhiteSpace(task.Tag) ? task.Tag : MessageConstants.UntitledTask;
        }
    }
}