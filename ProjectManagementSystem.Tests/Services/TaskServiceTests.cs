namespace ProjectManagementSystem.Tests.Services
{
    using BL.DTOs.Tasks;
    using BL.Enums;
    using BL.Enums.Task;
    using BL.Interfaces;
    using BL.Models;
    using BL.Services;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.Extensions.Logging;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Type = BL.Enums.Task.Type;

    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _mockTaskRepository;
        private readonly Mock<IProjectRepository> _mockProjectRepository;
        private readonly Mock<ICommentRepository> _mockCommentRepository;
        private readonly Mock<ITimeLogRepository> _mockTimeLogRepository;
        private readonly Mock<IActivityService> _mockActivityService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ILogger<TaskService>> _mockLogger;
        private readonly TaskService _service;

        public TaskServiceTests()
        {
            _mockTaskRepository = new Mock<ITaskRepository>();
            _mockProjectRepository = new Mock<IProjectRepository>();
            _mockCommentRepository = new Mock<ICommentRepository>();
            _mockTimeLogRepository = new Mock<ITimeLogRepository>();
            _mockActivityService = new Mock<IActivityService>();
            _mockLogger = new Mock<ILogger<TaskService>>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _service = new TaskService(
                _mockTaskRepository.Object,
                _mockProjectRepository.Object,
                _mockCommentRepository.Object,
                _mockTimeLogRepository.Object,
                _mockActivityService.Object,
                _mockUserManager.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetTasksByProjectAsync_ReturnsNull_WhenProjectNotFound()
        {
            _mockProjectRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Project?)null);

            var result = await _service.GetTasksByProjectAsync(1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTasksByProjectAsync_ReturnsTasksAndProjectName_WhenProjectExists()
        {
            var project = new Project { Id = 1, Name = "Project Alpha", CreatorId = "u1", CreatedAt = DateTime.UtcNow };
            var tasks = new List<ProjectTask>
            {
                new() { Id = 10, Title = "Task 1", Number = 1, Type = Type.Task, Priority = Priority.High, Status = Status.ToDo, ProjectId = 1, ReporterId = "u1", CreatedAt = DateTime.UtcNow, Assignee = new ApplicationUser { FirstName = "User", LastName = "A" } }
            };

            _mockProjectRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);
            _mockTaskRepository.Setup(r => r.GetTasksByProjectAsync(1)).ReturnsAsync(tasks);

            var result = await _service.GetTasksByProjectAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Project Alpha", result.Value.ProjectName);
            var taskList = result.Value.Tasks.ToList();
            Assert.Single(taskList);
            Assert.Equal("Task 1", taskList[0].Title);
            Assert.Equal("User A", taskList[0].AssigneeName);
        }

        [Fact]
        public async Task GetTaskForCreateAsync_ReturnsEmptyTaskDtoWithUsers()
        {
            var usersList = new List<ApplicationUser>
            {
                new() { Id = "u1", FirstName = "User", LastName = "One" },
                new() { Id = "u2", FirstName = "User", LastName = "Two" }
            };

            var mockQueryable = CreateMockQueryable(usersList);
            _mockUserManager.Setup(m => m.Users).Returns(mockQueryable.Object);

            var result = await _service.GetTaskForCreateAsync();

            Assert.NotNull(result);
            Assert.Empty(result.Title);
            Assert.Equal(Type.Task, result.Type);
            Assert.Equal(Priority.Low, result.Priority);
            Assert.Equal(Status.ToDo, result.Status);
            Assert.Equal(2, result.Users.Count);
            Assert.Equal("User One", result.Users[0].FullName);
        }

        [Fact]
        public async Task GetTaskForEditAsync_ReturnsNull_WhenTaskNotFound()
        {
            _mockTaskRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ProjectTask?)null);

            var result = await _service.GetTaskForEditAsync(1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTaskForEditAsync_ReturnsTaskDto_WhenTaskExists()
        {
            var task = new ProjectTask { Id = 1, Number = 2, Title = "Edit Task", Type = Type.Feature, Priority = Priority.Medium, Status = Status.InProgress, ProjectId = 1, ReporterId = "u1", CreatedAt = DateTime.UtcNow, AssigneeId = "u2" };
            var mockQueryable = CreateMockQueryable(new List<ApplicationUser>());

            _mockTaskRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
            _mockUserManager.Setup(m => m.Users).Returns(mockQueryable.Object);

            var result = await _service.GetTaskForEditAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Edit Task", result.Title);
            Assert.Equal("u2", result.AssigneeId);
        }

        [Fact]
        public async Task GetTaskForDeleteAsync_ReturnsNull_WhenTaskNotFound()
        {
            _mockTaskRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ProjectTask?)null);

            var result = await _service.GetTaskForDeleteAsync(1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTaskForDeleteAsync_ReturnsTaskDetailsDto_WhenTaskExists()
        {
            var task = new ProjectTask { Id = 1, Number = 5, Title = "Delete Task", Type = Type.Bug, Priority = Priority.Critical, Status = Status.ToDo, ProjectId = 1, ReporterId = "u1", CreatedAt = DateTime.UtcNow, Assignee = new ApplicationUser { FirstName = "Assignee", LastName = "User" }, Reporter = new ApplicationUser { FirstName = "Reporter", LastName = "User" } };
            _mockTaskRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

            var result = await _service.GetTaskForDeleteAsync(1);

            Assert.NotNull(result);
            Assert.Equal("BUG-5", result.Tag);
            Assert.Equal("Delete Task", result.Title);
            Assert.Equal("Assignee User", result.AssigneeName);
            Assert.Equal("Reporter User", result.ReporterName);
        }

        [Fact]
        public async Task GetTaskDetailsAsync_ReturnsNull_WhenTaskNotFound()
        {
            _mockTaskRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ProjectTask?)null);

            var result = await _service.GetTaskDetailsAsync(1, 1, "u1");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTaskDetailsAsync_ReturnsDetailsWithCommentsAndLogs_WhenTaskExists()
        {
            var author = new ApplicationUser { FirstName = "Author", LastName = "Name" };
            var task = new ProjectTask
            {
                Id = 1,
                Number = 12,
                Title = "Details Task",
                Type = Type.Task,
                Priority = Priority.High,
                Status = Status.InReview,
                ProjectId = 1,
                ReporterId = "u2",
                CreatedAt = DateTime.UtcNow,
                Reporter = new ApplicationUser { FirstName = "Reporter", LastName = "Name" },
                Assignee = null
            };
            var comments = new List<Comment>
            {
                new() { Id = 5, Content = "Nice working", UserId = "u1", User = author, TaskId = 1, CreatedAt = DateTime.UtcNow }
            };
            var timeLogs = new List<TimeLog>
            {
                new() { Id = 20, Hours = 3.5, Date = DateTime.UtcNow, Description = "Logs", TaskId = 1, UserId = "u1", User = author }
            };

            _mockTaskRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
            _mockCommentRepository.Setup(r => r.GetCommentsByTaskAsync(1)).ReturnsAsync(comments);
            _mockTimeLogRepository.Setup(r => r.GetTimeLogsByTaskAsync(1)).ReturnsAsync(timeLogs);

            var result = await _service.GetTaskDetailsAsync(1, 1, "u1");

            Assert.NotNull(result);
            Assert.Equal("Details Task", result.Title);
            Assert.Equal(3.5, result.TotalHours);
            Assert.Single(result.Comments);
            Assert.True(result.Comments[0].CanEdit);
            Assert.Single(result.TimeLogs);
            Assert.True(result.TimeLogs[0].CanEdit);
        }

        [Fact]
        public async Task GetTasksForBoardAsync_ReturnsNull_WhenProjectNotFound()
        {
            _mockProjectRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Project?)null);

            var result = await _service.GetTasksForBoardAsync(1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTasksForBoardAsync_ReturnsTasksAndProjectDetails_WhenProjectExists()
        {
            var project = new Project { Id = 1, Number = 4, Name = "Board Proj", CreatorId = "u1", CreatedAt = DateTime.UtcNow };
            var tasks = new List<ProjectTask>();
            _mockProjectRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);
            _mockTaskRepository.Setup(r => r.GetTasksByProjectAsync(1)).ReturnsAsync(tasks);

            var result = await _service.GetTasksForBoardAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Board Proj", result.Value.ProjectName);
            Assert.Equal("PRJ-4", result.Value.ProjectTag);
        }

        [Fact]
        public async Task CreateTaskAsync_SavesTaskAndLogsActivity()
        {
            var dto = new TaskDto { Title = "Add Auth", Type = Type.Task, Priority = Priority.High, Status = Status.ToDo };
            _mockTaskRepository.Setup(r => r.AddAsync(It.IsAny<ProjectTask>())).Returns(Task.CompletedTask);

            var result = await _service.CreateTaskAsync(1, dto, "user-123");

            Assert.True(result);
            _mockTaskRepository.Verify(r => r.AddAsync(It.Is<ProjectTask>(t => t.Title == "Add Auth" && t.ProjectId == 1 && t.ReporterId == "user-123")), Times.Once);
            _mockActivityService.Verify(a => a.LogAsync("user-123", It.Is<string>(s => s.Contains("Add Auth")), ActivityType.TaskAction), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskAsync_ReturnsFalse_WhenRepositoryUpdateFails()
        {
            var dto = new TaskDto { Title = "Fix UI", Type = Type.Bug, Priority = Priority.Low, Status = Status.InProgress };
            _mockTaskRepository.Setup(r => r.UpdateTaskAsync(1, It.IsAny<UpdateTaskDto>())).ReturnsAsync(false);

            var result = await _service.UpdateTaskAsync(1, dto, "user-123");

            Assert.False(result);
            _mockActivityService.Verify(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ActivityType>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTaskAsync_ReturnsTrueAndLogsActivity_WhenSuccessful()
        {
            var dto = new TaskDto { Title = "Fix UI", Type = Type.Bug, Priority = Priority.Low, Status = Status.InProgress };
            _mockTaskRepository.Setup(r => r.UpdateTaskAsync(1, It.IsAny<UpdateTaskDto>())).ReturnsAsync(true);

            var result = await _service.UpdateTaskAsync(1, dto, "user-123");

            Assert.True(result);
            _mockActivityService.Verify(a => a.LogAsync("user-123", It.Is<string>(s => s.Contains("Fix UI")), ActivityType.TaskAction), Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_ReturnsFalse_WhenRepositoryDeleteFails()
        {
            _mockTaskRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ProjectTask?)null);
            _mockTaskRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

            var result = await _service.DeleteTaskAsync(1, "user-123");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTaskAsync_ReturnsTrueAndLogsActivity_WhenSuccessful()
        {
            var task = new ProjectTask { Id = 1, Title = "Obsolete Feature", Type = Type.Task, Priority = Priority.Low, Status = Status.ToDo, ProjectId = 1, ReporterId = "u1", CreatedAt = DateTime.UtcNow };
            _mockTaskRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
            _mockTaskRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _service.DeleteTaskAsync(1, "user-123");

            Assert.True(result);
            _mockActivityService.Verify(a => a.LogAsync("user-123", It.Is<string>(s => s.Contains("Obsolete Feature")), ActivityType.TaskAction), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_ReturnsFalse_WhenStatusRepositoryFails()
        {
            _mockTaskRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ProjectTask?)null);
            _mockTaskRepository.Setup(r => r.UpdateStatusAsync(1, Status.Done)).ReturnsAsync(false);

            var result = await _service.UpdateTaskStatusAsync(1, Status.Done, "user-123");

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_ReturnsTrueAndLogsActivity_WhenSuccessful()
        {
            var task = new ProjectTask { Id = 1, Title = "Working Task", Number = 45, Type = Type.Task, Priority = Priority.Low, Status = Status.ToDo, ProjectId = 1, ReporterId = "u1", CreatedAt = DateTime.UtcNow };
            _mockTaskRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
            _mockTaskRepository.Setup(r => r.UpdateStatusAsync(1, Status.Done)).ReturnsAsync(true);

            var result = await _service.UpdateTaskStatusAsync(1, Status.Done, "user-123");

            Assert.True(result);
            _mockActivityService.Verify(a => a.LogAsync("user-123", It.Is<string>(s => s.Contains("TSK-45")), ActivityType.TaskAction), Times.Once);
        }

        private static Mock<IQueryable<T>> CreateMockQueryable<T>(IEnumerable<T> source)
        {
            var queryable = source.AsQueryable();
            var mock = new Mock<IQueryable<T>>();
            mock.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
            mock.Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            mock.Setup(m => m.Expression).Returns(queryable.Expression);
            mock.Setup(m => m.ElementType).Returns(queryable.ElementType);
            mock.Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            return mock;
        }

        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

            public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);

            public object? Execute(Expression expression) => _inner.Execute(expression);

            public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                var expectedResultType = typeof(TResult).GetGenericArguments()[0];
                var executionResult = typeof(IQueryProvider).GetMethods()
                    .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethod)
                    .MakeGenericMethod(expectedResultType)
                    .Invoke(_inner, [expression]);
                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(expectedResultType).Invoke(null, [executionResult])!;
            }
        }

        private class TestAsyncEnumerable<T>(Expression expression) : EnumerableQuery<T>(expression), IAsyncEnumerable<T>, IQueryable<T>
        {
            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        private class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner = inner;

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            { _inner.Dispose(); return ValueTask.CompletedTask; }

            public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
        }
    }
}