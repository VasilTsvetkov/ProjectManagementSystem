namespace ProjectManagementSystem.Services
{
    using Data;
    using Models;
    using Enums;
    using Interfaces;

    public class ActivityService : IActivityService
    {
        private readonly ApplicationDbContext _context;

        public ActivityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string userId, string message, ActivityType type)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}