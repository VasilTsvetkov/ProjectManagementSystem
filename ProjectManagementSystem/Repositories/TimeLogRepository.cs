namespace ProjectManagementSystem.Repositories
{
    using Data;
    using Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using DTOs.Dashboard;

    public class TimeLogRepository : Repository<TimeLog>, ITimeLogRepository
    {
        private readonly ApplicationDbContext _context;

        public TimeLogRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TimeLog>> GetTimeLogsByTaskAsync(int taskId)
            => await _context.TimeLogs
                .Include(t => t.User)
                .Where(t => t.TaskId == taskId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

        public async Task<IEnumerable<TimeLog>> GetTimeLogsByUserAsync(string userId)
            => await _context.TimeLogs
                .Include(t => t.Task)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

        public async Task<IEnumerable<TimeLog>> GetTimeLogsByProjectAsync(int projectId)
            => await _context.TimeLogs
                .Include(t => t.User)
                .Include(t => t.Task)
                .Where(t => t.Task.ProjectId == projectId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();

        public async Task<IEnumerable<TimeLog>> GetByMonthAsync(int year, int month, string? userId = null)
        {
            var query = _context.TimeLogs
                .Include(tl => tl.User)
                .Include(tl => tl.Task)
                    .ThenInclude(t => t.Project)
                .Where(tl => tl.Date.Year == year && tl.Date.Month == month);

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(tl => tl.UserId == userId);
            }

            return await query.OrderByDescending(tl => tl.Date).ToListAsync();
        }

        public async Task<MonthlyStatsDto> GetMonthlyStatsAsync(int year, int month, string? userId = null)
        {
            var logs = await GetByMonthAsync(year, month, userId);

            return new MonthlyStatsDto
            {
                TotalHours = logs.Sum(tl => tl.Hours),
                TotalProjects = logs.Select(tl => tl.Task.ProjectId).Distinct().Count(),
                TotalTasks = logs.Select(tl => tl.TaskId).Distinct().Count(),
                TotalLogs = logs.Count()
            };
        }

        public async Task<IEnumerable<ProjectTimeDto>> GetProjectBreakdownAsync(int year, int month, string? userId = null)
        {
            var logs = await GetByMonthAsync(year, month, userId);

            return logs
                .GroupBy(tl => new
                {
                    tl.Task.Project.Id,
                    tl.Task.Project.Name,
                    tl.Task.Project.Tag
                })
                .Select(g => new ProjectTimeDto
                {
                    ProjectId = g.Key.Id,
                    ProjectName = g.Key.Name,
                    ProjectTag = g.Key.Tag,
                    TotalHours = g.Sum(tl => tl.Hours),
                    TaskCount = g.Select(tl => tl.TaskId).Distinct().Count(),
                    LogCount = g.Count()
                })
                .OrderByDescending(p => p.TotalHours)
                .ToList();
        }

        public async Task<IEnumerable<UserTimeDto>> GetUserBreakdownAsync(int year, int month, int? projectId = null)
        {
            var query = _context.TimeLogs
                .Include(tl => tl.User)
                .Include(tl => tl.Task)
                    .ThenInclude(t => t.Project)
                .Where(tl => tl.Date.Year == year && tl.Date.Month == month);

            if (projectId.HasValue)
            {
                query = query.Where(tl => tl.Task.ProjectId == projectId.Value);
            }

            var logs = await query.ToListAsync();

            return logs
                .GroupBy(tl => new { tl.UserId, tl.User.FirstName, tl.User.LastName })
                .Select(g => new UserTimeDto
                {
                    UserId = g.Key.UserId,
                    UserName = $"{g.Key.FirstName} {g.Key.LastName}",
                    TotalHours = g.Sum(tl => tl.Hours),
                    ProjectCount = g.Select(tl => tl.Task.ProjectId).Distinct().Count(),
                    TaskCount = g.Select(tl => tl.TaskId).Distinct().Count()
                })
                .OrderByDescending(u => u.TotalHours)
                .ToList();
        }
    }
}