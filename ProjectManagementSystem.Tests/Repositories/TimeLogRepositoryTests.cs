namespace ProjectManagementSystem.Tests.Repositories
{
    using BL.Data;
    using BL.Models;
    using BL.Repositories;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class TimeLogRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeLogRepository _repository;

        public TimeLogRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new TimeLogRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task SeedDataAsync()
        {
            var u1 = new ApplicationUser { Id = "u1", FirstName = "Ivan", LastName = "Ivanov" };
            var u2 = new ApplicationUser { Id = "u2", FirstName = "Petar", LastName = "Petrov" };

            var p1 = new Project { Id = 1, Number = 1, Name = "P1", CreatorId = "u1", CreatedAt = DateTime.UtcNow };
            var p2 = new Project { Id = 2, Number = 2, Name = "P2", CreatorId = "u1", CreatedAt = DateTime.UtcNow };

            var t1 = new ProjectTask
            {
                Id = 10,
                Number = 1,
                Title = "T1",
                ProjectId = 1,
                ReporterId = "u1",
                Type = BL.Enums.Task.Type.Task,
                Priority = BL.Enums.Task.Priority.Medium,
                Status = BL.Enums.Task.Status.ToDo,
                CreatedAt = DateTime.UtcNow
            };
            var t2 = new ProjectTask
            {
                Id = 20,
                Number = 2,
                Title = "T2",
                ProjectId = 2,
                ReporterId = "u1",
                Type = BL.Enums.Task.Type.Bug,
                Priority = BL.Enums.Task.Priority.High,
                Status = BL.Enums.Task.Status.InProgress,
                CreatedAt = DateTime.UtcNow
            };

            var logs = new List<TimeLog>
            {
                new() { Id = 100, Hours = 4.5, Date = new DateTime(2026, 6, 5), TaskId = 10, UserId = "u1" },
                new() { Id = 101, Hours = 3.0, Date = new DateTime(2026, 6, 15), TaskId = 10, UserId = "u2" },
                new() { Id = 102, Hours = 6.0, Date = new DateTime(2026, 6, 12), TaskId = 20, UserId = "u1" },
                new() { Id = 103, Hours = 2.0, Date = new DateTime(2026, 5, 20), TaskId = 10, UserId = "u1" }
            };

            await _context.Users.AddRangeAsync(u1, u2);
            await _context.Projects.AddRangeAsync(p1, p2);
            await _context.Tasks.AddRangeAsync(t1, t2);
            await _context.TimeLogs.AddRangeAsync(logs);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetTimeLogsByTaskAsync_ReturnsLogsOrderedByDateDescending()
        {
            await SeedDataAsync();

            var result = (await _repository.GetTimeLogsByTaskAsync(10)).ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal(101, result[0].Id);
            Assert.Equal(100, result[1].Id);
            Assert.Equal(103, result[2].Id);
            Assert.NotNull(result[0].User);
        }

        [Fact]
        public async Task GetTimeLogsByUserAsync_ReturnsLogsForTargetUserOrderedByDateDescending()
        {
            await SeedDataAsync();

            var result = (await _repository.GetTimeLogsByUserAsync("u1")).ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal(102, result[0].Id);
            Assert.Equal(100, result[1].Id);
            Assert.Equal(103, result[2].Id);
            Assert.NotNull(result[0].Task);
        }

        [Fact]
        public async Task GetTimeLogsByProjectAsync_ReturnsLogsForTargetProjectOrderedByDateDescending()
        {
            await SeedDataAsync();

            var result = (await _repository.GetTimeLogsByProjectAsync(1)).ToList();

            Assert.Equal(3, result.Count);
            Assert.True(result.All(l => l.Task.ProjectId == 1));
            Assert.Equal(101, result[0].Id);
            Assert.NotNull(result[0].User);
            Assert.NotNull(result[0].Task);
        }

        [Fact]
        public async Task GetByMonthAsync_FiltersByYearAndMonthAndOptionalUserId()
        {
            await SeedDataAsync();

            var generalResult = (await _repository.GetByMonthAsync(2026, 6)).ToList();
            Assert.Equal(3, generalResult.Count);
            Assert.Equal(101, generalResult[0].Id);

            var filteredResult = (await _repository.GetByMonthAsync(2026, 6, "u2")).ToList();
            var log = Assert.Single(filteredResult);
            Assert.Equal(101, log.Id);
            Assert.NotNull(log.Task.Project);
        }

        [Fact]
        public async Task GetMonthlyStatsAsync_CalculatesAggregatesCorrectly()
        {
            await SeedDataAsync();

            var stats = await _repository.GetMonthlyStatsAsync(2026, 6);

            Assert.Equal(13.5, stats.TotalHours);
            Assert.Equal(2, stats.TotalProjects);
            Assert.Equal(2, stats.TotalTasks);
            Assert.Equal(3, stats.TotalLogs);
        }

        [Fact]
        public async Task GetProjectBreakdownAsync_GroupsAndOrdersByHoursDescending()
        {
            await SeedDataAsync();

            var result = (await _repository.GetProjectBreakdownAsync(2026, 6)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].ProjectId);
            Assert.Equal(7.5, result[0].TotalHours);
            Assert.Equal(1, result[0].TaskCount);
            Assert.Equal(2, result[0].LogCount);

            Assert.Equal(2, result[1].ProjectId);
            Assert.Equal(6.0, result[1].TotalHours);
        }

        [Fact]
        public async Task GetUserBreakdownAsync_GroupsByUserAndFiltersByOptionalProjectId()
        {
            await SeedDataAsync();

            var generalUsers = (await _repository.GetUserBreakdownAsync(2026, 6)).ToList();
            Assert.Equal(2, generalUsers.Count);
            Assert.Equal("u1", generalUsers[0].UserId);
            Assert.Equal(10.5, generalUsers[0].TotalHours);

            var filteredUsers = (await _repository.GetUserBreakdownAsync(2026, 6, projectId: 2)).ToList();
            var userRow = Assert.Single(filteredUsers);
            Assert.Equal("u1", userRow.UserId);
            Assert.Equal(6.0, userRow.TotalHours);
            Assert.Equal(1, userRow.ProjectCount);
            Assert.Equal(1, userRow.TaskCount);
        }

        [Fact]
        public async Task GetLogsByProjectAndDateAsync_FiltersBoundariesCorrectly()
        {
            await SeedDataAsync();

            var start = new DateTime(2026, 6, 5);
            var end = new DateTime(2026, 6, 12);

            var result = (await _repository.GetLogsByProjectAndDateAsync(1, start, end)).ToList();

            Assert.Single(result);
            Assert.Equal(100, result[0].Id);
        }
    }
}