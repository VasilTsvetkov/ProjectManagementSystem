namespace ProjectManagementSystem.Tests.Services
{
    using BL.Constants;
    using BL.DTOs;
    using BL.Enums;
    using BL.Enums.Task;
    using BL.Interfaces;
    using BL.Models;
    using BL.Services;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using Type = BL.Enums.Task.Type;

    public class CommentServiceTests
    {
        private readonly Mock<ICommentRepository> _mockCommentRepository;
        private readonly Mock<ITaskRepository> _mockTaskRepository;
        private readonly Mock<IActivityService> _mockActivityService;
        private readonly Mock<ILogger<CommentService>> _mockLogger;
        private readonly CommentService _service;

        public CommentServiceTests()
        {
            _mockCommentRepository = new Mock<ICommentRepository>();
            _mockTaskRepository = new Mock<ITaskRepository>();
            _mockActivityService = new Mock<IActivityService>();
            _mockLogger = new Mock<ILogger<CommentService>>();

            _service = new CommentService(
                _mockCommentRepository.Object,
                _mockTaskRepository.Object,
                _mockActivityService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CreateCommentAsync_SavesCommentAndLogsActivity_WithTaskTag()
        {
            var dto = new CommentDto { Content = "New comment context", AuthorName = "Test Author", TaskId = 1 };
            var task = new ProjectTask
            {
                Id = 1,
                Number = 15,
                Title = "Test Task",
                Type = Type.Task,
                Priority = Priority.Medium,
                Status = Status.ToDo,
                ProjectId = 2,
                ReporterId = "reporter-id",
                CreatedAt = DateTime.UtcNow,
                Project = new Project { Id = 2, Number = 2, Name = "Test Project", CreatorId = "u1", CreatedAt = DateTime.UtcNow }
            };

            _mockCommentRepository.Setup(r => r.AddAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);
            _mockTaskRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

            var result = await _service.CreateCommentAsync(dto, "user-123");

            Assert.True(result);
            _mockCommentRepository.Verify(r => r.AddAsync(It.Is<Comment>(c => c.Content == "New comment context" && c.TaskId == 1 && c.UserId == "user-123")), Times.Once);
            _mockActivityService.Verify(a => a.LogAsync("user-123", It.Is<string>(s => s.Contains("TSK-15")), ActivityType.CommentAction), Times.Once);
        }

        [Fact]
        public async Task CreateCommentAsync_UsesFallbackIdentifier_WhenTaskNotFound()
        {
            var dto = new CommentDto { Content = "Orphan comment", AuthorName = "Test Author", TaskId = 999 };
            _mockCommentRepository.Setup(r => r.AddAsync(It.IsAny<Comment>())).Returns(Task.CompletedTask);
            _mockTaskRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ProjectTask?)null);

            var result = await _service.CreateCommentAsync(dto, "user-123");

            Assert.True(result);
            _mockActivityService.Verify(a => a.LogAsync("user-123", It.Is<string>(s => s.Contains(MessageConstants.MissingTaskIdentifier)), ActivityType.CommentAction), Times.Once);
        }

        [Fact]
        public async Task GetCommentForEditAsync_ReturnsNull_WhenCommentNotFound()
        {
            _mockCommentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Comment?)null);

            var result = await _service.GetCommentForEditAsync(1, "user-123");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetCommentForEditAsync_ReturnsNull_WhenUserIsNotAuthor()
        {
            var comment = new Comment
            {
                Id = 1,
                Content = "Content",
                TaskId = 1,
                UserId = "author-id",
                CreatedAt = DateTime.UtcNow
            };
            _mockCommentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

            var result = await _service.GetCommentForEditAsync(1, "different-user");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetCommentForEditAsync_ReturnsCommentDto_WhenValid()
        {
            var comment = new Comment
            {
                Id = 1,
                Content = "Text",
                TaskId = 10,
                UserId = "user-1",
                CreatedAt = DateTime.UtcNow,
                Task = new ProjectTask
                {
                    Id = 10,
                    Number = 1,
                    Title = "Task title",
                    Type = Type.Task,
                    Priority = Priority.Low,
                    Status = Status.ToDo,
                    ProjectId = 5,
                    ReporterId = "user-1",
                    CreatedAt = DateTime.UtcNow,
                    Project = new Project { Id = 5, Number = 5, Name = "Proj", CreatorId = "u1", CreatedAt = DateTime.UtcNow }
                },
                User = new ApplicationUser { FirstName = "Ivan", LastName = "Ivanov" }
            };
            _mockCommentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

            var result = await _service.GetCommentForEditAsync(1, "user-1");

            Assert.NotNull(result);
            Assert.Equal("Text", result.Content);
            Assert.Equal(5, result.ProjectId);
            Assert.Equal("Ivan Ivanov", result.AuthorName);
            Assert.True(result.CanEdit);
        }

        [Fact]
        public async Task UpdateCommentAsync_ReturnsFalse_WhenUnauthorizedOrMissing()
        {
            var comment = new Comment
            {
                Id = 1,
                Content = "Content",
                TaskId = 1,
                UserId = "user-1",
                CreatedAt = DateTime.UtcNow
            };
            _mockCommentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);

            var result = await _service.UpdateCommentAsync(1, new CommentDto { Content = "New", AuthorName = "Test Author" }, "wrong-user");

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateCommentAsync_ReturnsTrueAndLogs_WhenSuccessful()
        {
            var comment = new Comment
            {
                Id = 1,
                Content = "Old content",
                TaskId = 2,
                UserId = "user-1",
                CreatedAt = DateTime.UtcNow
            };
            var task = new ProjectTask
            {
                Id = 2,
                Number = 4,
                Title = "Task 4",
                Type = Type.Task,
                Priority = Priority.Low,
                Status = Status.ToDo,
                ProjectId = 1,
                ReporterId = "user-1",
                CreatedAt = DateTime.UtcNow,
                Project = new Project { Id = 1, Number = 1, Name = "P1", CreatorId = "u1", CreatedAt = DateTime.UtcNow }
            };

            _mockCommentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
            _mockCommentRepository.Setup(r => r.UpdateCommentAsync(1, "Updated text")).ReturnsAsync(true);
            _mockTaskRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(task);

            var result = await _service.UpdateCommentAsync(1, new CommentDto { Content = "Updated text", AuthorName = "Test Author" }, "user-1");

            Assert.True(result);
            _mockActivityService.Verify(a => a.LogAsync("user-1", It.Is<string>(s => s.Contains("TSK-4")), ActivityType.CommentAction), Times.Once);
        }

        [Fact]
        public async Task DeleteCommentAsync_ReturnsNull_WhenCommentMissingOrUnauthorized()
        {
            _mockCommentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Comment?)null);

            var result = await _service.DeleteCommentAsync(1, "user-1");

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteCommentAsync_ReturnsTuple_WhenSuccessfullyDeleted()
        {
            var comment = new Comment
            {
                Id = 1,
                Content = "To delete",
                TaskId = 20,
                UserId = "user-1",
                CreatedAt = DateTime.UtcNow
            };
            var task = new ProjectTask
            {
                Id = 20,
                Number = 7,
                Title = "Task 7",
                Type = Type.Task,
                Priority = Priority.Low,
                Status = Status.ToDo,
                ProjectId = 1,
                ReporterId = "user-1",
                CreatedAt = DateTime.UtcNow,
                Project = new Project { Id = 1, Number = 1, Name = "P1", CreatorId = "u1", CreatedAt = DateTime.UtcNow }
            };

            _mockCommentRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(comment);
            _mockCommentRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);
            _mockTaskRepository.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(task);

            var result = await _service.DeleteCommentAsync(1, "user-1");

            Assert.NotNull(result);
            Assert.True(result.Value.Success);
            Assert.Equal(20, result.Value.TaskId);
            _mockActivityService.Verify(a => a.LogAsync("user-1", It.Is<string>(s => s.Contains("TSK-7")), ActivityType.CommentAction), Times.Once);
        }
    }
}