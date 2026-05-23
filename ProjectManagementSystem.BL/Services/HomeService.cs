namespace ProjectManagementSystem.BL.Services
{
    using Constants;
    using Data;
    using DTOs;
    using DTOs.Tasks;
    using Enums.Task;
    using Interfaces;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class HomeService(ApplicationDbContext context) : IHomeService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<HomeIndexDto> GetHomeIndexDataAsync(string userId)
        {
            var myAssignedTasksQuery = _context.Tasks
                .Where(t => t.AssigneeId == userId && t.Status != Status.Done);

            var pendingCount = await myAssignedTasksQuery.CountAsync();
            var overdueCount = await myAssignedTasksQuery.CountAsync(t => t.Deadline < DateTime.UtcNow);

            var urgentTasks = await myAssignedTasksQuery
                 .OrderByDescending(t => t.Priority)
                 .ThenBy(t => t.Deadline)
                 .Take(5)
                 .Select(t => new TaskDto
                 {
                     Id = t.Id,
                     ProjectId = t.ProjectId,
                     Tag = t.Tag,
                     Title = t.Title,
                     Type = t.Type,
                     Priority = t.Priority,
                     Status = t.Status,
                     Deadline = t.Deadline,
                     AssigneeId = t.AssigneeId,
                     AssigneeName = t.Assignee != null ? t.Assignee.FullName : MessageConstants.Unassigned
                 }).ToListAsync();

            var recentActivities = await _context.ActivityLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .Take(5)
                .Select(a => new ActivityDto
                {
                    Message = a.Message,
                    Timestamp = a.Timestamp,
                    Type = a.Type
                }).ToListAsync();

            return new HomeIndexDto
            {
                MyPendingTasksCount = pendingCount,
                OverdueTasksCount = overdueCount,
                UrgentTasks = urgentTasks,
                RecentActivities = recentActivities
            };
        }
    }
}