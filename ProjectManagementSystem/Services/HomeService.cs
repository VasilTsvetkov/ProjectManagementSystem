namespace ProjectManagementSystem.Services
{
    using Data;
    using Interfaces;
    using ViewModels.Home;
    using ViewModels.Tasks;
    using Enums;
    using Microsoft.EntityFrameworkCore;

    public class HomeService(ApplicationDbContext context) : IHomeService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IndexViewModel> GetHomeIndexDataAsync(string userId)
        {
            var myAssignedTasksQuery = _context.Tasks
                .Where(t => t.AssigneeId == userId && t.Status != ProjectTaskStatus.Done);

            var pendingCount = await myAssignedTasksQuery.CountAsync();
            var overdueCount = await myAssignedTasksQuery.CountAsync(t => t.Deadline < DateTime.UtcNow);

            var urgentTasks = await myAssignedTasksQuery
                 .OrderByDescending(t => t.Priority)
                 .ThenBy(t => t.Deadline)
                 .Take(5)
                 .Select(t => new TaskListViewModel
                 {
                     Id = t.Id,
                     ProjectId = t.ProjectId,
                     Tag = t.Tag,
                     Title = t.Title,
                     Type = t.Type,
                     Priority = t.Priority,
                     Status = t.Status,
                     Deadline = t.Deadline
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

            return new IndexViewModel
            {
                MyPendingTasksCount = pendingCount,
                OverdueTasksCount = overdueCount,
                UrgentTasks = urgentTasks,
                RecentActivities = recentActivities
            };
        }
    }
}