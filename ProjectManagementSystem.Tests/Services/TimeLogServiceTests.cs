namespace ProjectManagementSystem.Tests.Services
{
    using BL.DTOs.TimeLogs;
    using BL.Enums;
    using BL.Enums.Task;
    using BL.Interfaces;
    using BL.Models;
    using BL.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;
    using Type = BL.Enums.Task.Type;

    public class TimeLogServiceTests
    {
        private readonly Mock<ITimeLogRepository> _mockTimeLogRepository;
        private readonly Mock<ITaskRepository> _mockTaskRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<IActivityService> _mockActivityService;
        private readonly Mock<ILogger<TimeLogService>> _mockLogger;
        private readonly TimeLogService _service;

        public TimeLogServiceTests()
        {
            _mockTimeLogRepository = new Mock<ITimeLogRepository>();
            _mockTaskRepository = new Mock<ITaskRepository>();
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockActivityService = new Mock<IActivityService>();
            _mockLogger = new Mock<ILogger<TimeLogService>>();

            _service = new TimeLogService(
                _mockTimeLogRepository.Object,
                _mockTaskRepository.Object,
                _mockProjectRepository.Object,
                _mockActivityService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CreateTimeLogAsync_ReturnsFalse_WhenDateIsInFuture()
        {
            var dto = new TimeLogDto
            {
                ProjectId = 1,
                TaskId = 2,
                Days = 0,
                Hours = 4,
                Minutes = 0,
                Date = DateTime.UtcNow.AddDays(1),
                Description = "Future work"
            };

            var result = await _service.CreateTimeLogAsync(dto, "user-1");

            Assert.False(result);
        }

        [Fact]
        public async Task CreateTimeLogAsync_ReturnsFalse_WhenTotalLoggedTimeIsZero()
        {
            var dto = new TimeLogDto
            {
                ProjectId = 1,
                TaskId = 2,
                Days = 0,
                Hours = 0,
                Minutes = 0,
                Date = DateTime.UtcNow,
                Description = "Zero work"
            };

            var result = await _service.CreateTimeLogAsync(dto, "user-1");

            Assert.False(result);
        }

        [Fact]
        public async Task CreateTimeLogAsync_ReturnsFalse_WhenDailyLimitExceeded()
        {
            var dto = new TimeLogDto
            {
                ProjectId = 1,
                TaskId = 2,
                Days = 0,
                Hours = 5,
                Minutes = 0,
                Date = DateTime.UtcNow.Date,
                Description = "Extra work"
            };

            var existingLogs = new List<TimeLog>
            {
                new() { Id = 10, Hours = 4.0, Date = dto.Date, TaskId = 2, UserId = "user-1" }
            };

            _mockTimeLogRepository.Setup(r => r.GetLogsByProjectAndDateAsync(1, dto.Date, dto.Date))
                .ReturnsAsync(existingLogs);

            var result = await _service.CreateTimeLogAsync(dto, "user-1");

            Assert.False(result);
        }

        [Fact]
        public async Task CreateTimeLogAsync_SavesLogAndLogsActivity_WhenValid()
        {
            var today = DateTime.UtcNow.Date;
            var dto = new TimeLogDto
            {
                ProjectId = 1,
                TaskId = 2,
                Days = 0,
                Hours = 4,
                Minutes = 30,
                Date = today,
                Description = "Coding tests"
            };

            var task = new ProjectTask
            {
                Id = 2,
                Number = 101,
                Title = "Write Tests",
                Type = Type.Task,
                Priority = Priority.High,
                Status = Status.InProgress,
                ProjectId = 1,
                ReporterId = "user-1",
                CreatedAt = DateTime.UtcNow,
                Project = new Project { Id = 1, Number = 1, Name = "Proj 1", CreatorId = "user-1", CreatedAt = DateTime.UtcNow }
            };

            _mockTimeLogRepository.Setup(r => r.GetLogsByProjectAndDateAsync(1, today, today))
                .ReturnsAsync([]);
            _mockTimeLogRepository.Setup(r => r.AddAsync(It.IsAny<TimeLog>())).Returns(Task.CompletedTask);
            _mockTaskRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(task);

            var result = await _service.CreateTimeLogAsync(dto, "user-1");

            Assert.True(result);
            _mockTimeLogRepository.Verify(r => r.AddAsync(It.Is<TimeLog>(l => l.Hours == 4.5 && l.UserId == "user-1" && l.TaskId == 2)), Times.Once);
            _mockActivityService.Verify(a => a.LogAsync("user-1", It.Is<string>(s => s.Contains("TSK-101")), ActivityType.TimeLogAction), Times.Once);
        }

        [Fact]
        public async Task DeleteTimeLogAsync_ReturnsNull_WhenLogMissingOrUnauthorized()
        {
            _mockTimeLogRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((TimeLog?)null);

            var result = await _service.DeleteTimeLogAsync(1, "user-1");

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteTimeLogAsync_ReturnsTuple_WhenSuccessfullyDeleted()
        {
            var log = new TimeLog { Id = 1, Hours = 2.25, Date = DateTime.UtcNow, TaskId = 5, UserId = "user-1" };
            var task = new ProjectTask
            {
                Id = 5,
                Number = 42,
                Title = "Fix Bug",
                Type = Type.Bug,
                Priority = Priority.Critical,
                Status = Status.ToDo,
                ProjectId = 1,
                ReporterId = "user-1",
                CreatedAt = DateTime.UtcNow,
                Project = new Project { Id = 1, Number = 1, Name = "P1", CreatorId = "u1", CreatedAt = DateTime.UtcNow }
            };

            _mockTimeLogRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(log);
            _mockTimeLogRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);
            _mockTaskRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(task);

            var result = await _service.DeleteTimeLogAsync(1, "user-1");

            Assert.NotNull(result);
            Assert.True(result.Value.Success);
            Assert.Equal(5, result.Value.TaskId);
            _mockActivityService.Verify(a => a.LogAsync("user-1", It.Is<string>(s => s.Contains("BUG-42")), ActivityType.TimeLogAction), Times.Once);
        }

        [Fact]
        public async Task GetMonthlyMatrixAsync_ReturnsNull_WhenProjectNotFound()
        {
            _mockProjectRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Project?)null);

            var result = await _service.GetMonthlyMatrixAsync(1, 5, 2026);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetMonthlyMatrixAsync_PopulatesMatrixCorrectly_WhenLogsExist()
        {
            var project = new Project { Id = 1, Number = 1, Name = "Alpha Project", CreatorId = "user-1", CreatedAt = DateTime.UtcNow };
            var user = new ApplicationUser { FirstName = "Nikola", LastName = "Tesla" };

            var logs = new List<TimeLog>
            {
                new() { Id = 1, Hours = 4.0, Date = new DateTime(2026, 6, 5), TaskId = 1, UserId = "u-tesla", User = user },
                new() { Id = 2, Hours = 3.5, Date = new DateTime(2026, 6, 5), TaskId = 2, UserId = "u-tesla", User = user },
                new() { Id = 3, Hours = 6.0, Date = new DateTime(2026, 6, 12), TaskId = 1, UserId = "u-tesla", User = user }
            };

            _mockProjectRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);
            _mockTimeLogRepository.Setup(r => r.GetLogsByProjectAndDateAsync(1, new DateTime(2026, 6, 1), new DateTime(2026, 6, 30)))
                .ReturnsAsync(logs);

            var result = await _service.GetMonthlyMatrixAsync(1, 6, 2026);

            Assert.NotNull(result);
            Assert.Equal(1, result.ProjectId);
            Assert.Equal("Alpha Project", result.ProjectName);
            Assert.Equal(30, result.DaysInMonth.Count);

            var row = Assert.Single(result.Rows);
            Assert.Equal("u-tesla", row.UserId);
            Assert.Equal("Nikola Tesla", row.FullName);

            Assert.Equal(7.5, row.DailyHours[5]);
            Assert.Equal(6.0, row.DailyHours[12]);
            Assert.False(row.DailyHours.ContainsKey(1));
        }
    }
}