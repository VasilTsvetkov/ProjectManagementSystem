namespace ProjectManagementSystem.Repositories
{
    using Data;
    using Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Models;

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
    }
}