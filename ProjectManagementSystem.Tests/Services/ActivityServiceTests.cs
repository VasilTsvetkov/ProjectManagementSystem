namespace ProjectManagementSystem.Tests.Services
{
    using BL.Enums;
    using BL.Services;
    using Microsoft.EntityFrameworkCore;
    using ProjectManagementSystem.BL.Data;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class ActivityServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public ActivityServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task LogAsync_SavesActivityLogToDatabase()
        {
            await using var context = new ApplicationDbContext(_dbOptions);
            var service = new ActivityService(context);
            const string userId = "user-123";
            const string message = "Created a new project task";
            const ActivityType type = ActivityType.TaskAction;

            await service.LogAsync(userId, message, type);

            await using var verifyContext = new ApplicationDbContext(_dbOptions);
            var log = await verifyContext.ActivityLogs.FirstOrDefaultAsync();

            Assert.NotNull(log);
            Assert.Equal(userId, log.UserId);
            Assert.Equal(message, log.Message);
            Assert.Equal(type, log.Type);
            Assert.True((DateTime.UtcNow - log.Timestamp).TotalSeconds < 5);
        }
    }
}