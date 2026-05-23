namespace ProjectManagementSystem.Core.Services
{
    using Data;
    using Enums;
    using Interfaces;
    using Models;

    public class ActivityService(ApplicationDbContext context) : IActivityService
    {
        private readonly ApplicationDbContext _context = context;

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