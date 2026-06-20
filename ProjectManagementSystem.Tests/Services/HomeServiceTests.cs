namespace ProjectManagementSystem.Tests.Services
{
    using BL.Data;
    using BL.Enums;
    using BL.Enums.Task;
    using BL.Models;
    using BL.Services;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;
    using Type = BL.Enums.Task.Type;

    public class HomeServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly HomeService _service;

        public HomeServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _service = new HomeService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task GetHomeIndexDataAsync_CalculatesCountsAndProjectionsCorrectly()
        {
            const string targetUserId = "user-1";
            var user = new ApplicationUser { Id = targetUserId, FirstName = "Vasil", LastName = "Vasilev" };

            var project = new Project
            {
                Id = 1,
                Number = 1,
                Name = "Core Project",
                CreatorId = targetUserId,
                CreatedAt = DateTime.UtcNow
            };

            var tasks = new List<ProjectTask>
            {
                new()
                {
                    Id = 10, Number = 1, Title = "Urgent Task", Type = Type.Task,
                    Priority = Priority.Critical, Status = Status.ToDo, ProjectId = 1,
                    ReporterId = targetUserId, AssigneeId = targetUserId, CreatedAt = DateTime.UtcNow,
                    Deadline = DateTime.UtcNow.AddDays(-2)
                },
                new()
                {
                    Id = 11, Number = 2, Title = "Normal Task", Type = Type.Feature,
                    Priority = Priority.Medium, Status = Status.InProgress, ProjectId = 1,
                    ReporterId = targetUserId, AssigneeId = targetUserId, CreatedAt = DateTime.UtcNow,
                    Deadline = DateTime.UtcNow.AddDays(5)
                },
                new()
                {
                    Id = 12, Number = 3, Title = "Completed Task", Type = Type.Bug,
                    Priority = Priority.High, Status = Status.Done, ProjectId = 1,
                    ReporterId = targetUserId, AssigneeId = targetUserId, CreatedAt = DateTime.UtcNow,
                    Deadline = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    Id = 13, Number = 4, Title = "Unassigned Task", Type = Type.Task,
                    Priority = Priority.High, Status = Status.ToDo, ProjectId = 1,
                    ReporterId = targetUserId, AssigneeId = null, CreatedAt = DateTime.UtcNow
                }
            };

            var logs = new List<ActivityLog>
            {
                new() { Id = 1, Message = "Created task", Timestamp = DateTime.UtcNow.AddMinutes(-10), Type = ActivityType.TaskAction, UserId = targetUserId },
                new() { Id = 2, Message = "Added comment", Timestamp = DateTime.UtcNow.AddMinutes(-5), Type = ActivityType.CommentAction, UserId = targetUserId },
                new() { Id = 3, Message = "Other user log", Timestamp = DateTime.UtcNow.AddMinutes(-2), Type = ActivityType.TaskAction, UserId = "user-2" }
            };

            await _context.Users.AddAsync(user);
            await _context.Projects.AddAsync(project);
            await _context.Tasks.AddRangeAsync(tasks);
            await _context.ActivityLogs.AddRangeAsync(logs);
            await _context.SaveChangesAsync();

            var result = await _service.GetHomeIndexDataAsync(targetUserId);

            Assert.NotNull(result);
            Assert.Equal(2, result.MyPendingTasksCount);
            Assert.Equal(1, result.OverdueTasksCount);

            Assert.Equal(2, result.UrgentTasks.Count);
            var topTask = result.UrgentTasks[0];
            Assert.Equal(10, topTask.Id);
            Assert.Equal("Vasil Vasilev", topTask.AssigneeName);

            Assert.Equal(2, result.RecentActivities.Count);
            Assert.Equal("Added comment", result.RecentActivities[0].Message);
        }
    }
}