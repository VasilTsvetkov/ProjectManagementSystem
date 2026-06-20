namespace ProjectManagementSystem.Tests.Repositories
{
    using BL.Data;
    using BL.DTOs.Tasks;
    using BL.Enums.Task;
    using BL.Models;
    using BL.Repositories;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class TaskRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TaskRepository _repository;

        public TaskRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new TaskRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task GetTasksByProjectAsync_ReturnsTasksForTargetProjectWithRelationships()
        {
            var u1 = new ApplicationUser { Id = "u1", FirstName = "Ivan", LastName = "Ivanov" };
            var u2 = new ApplicationUser { Id = "u2", FirstName = "Petar", LastName = "Petrov" };
            var project = new Project { Id = 1, Number = 1, Name = "P1", CreatorId = "u1", CreatedAt = DateTime.UtcNow };

            var t1 = new ProjectTask
            {
                Id = 10,
                Number = 1,
                Title = "T1",
                ProjectId = 1,
                ReporterId = "u1",
                AssigneeId = "u2",
                Type = BL.Enums.Task.Type.Task,
                Priority = Priority.Medium,
                Status = Status.ToDo,
                CreatedAt = DateTime.UtcNow
            };
            var t2 = new ProjectTask
            {
                Id = 11,
                Number = 2,
                Title = "T2",
                ProjectId = 2,
                ReporterId = "u1",
                AssigneeId = "u2",
                Type = BL.Enums.Task.Type.Task,
                Priority = Priority.Medium,
                Status = Status.ToDo,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddRangeAsync(u1, u2);
            await _context.Projects.AddAsync(project);
            await _context.Tasks.AddRangeAsync(t1, t2);
            await _context.SaveChangesAsync();

            var result = (await _repository.GetTasksByProjectAsync(1)).ToList();

            var task = Assert.Single(result);
            Assert.Equal(10, task.Id);
            Assert.NotNull(task.Reporter);
            Assert.NotNull(task.Assignee);
        }

        [Fact]
        public async Task GetTasksByAssigneeAsync_ReturnsTasksForTargetAssigneeWithProject()
        {
            var u1 = new ApplicationUser { Id = "u1", FirstName = "Ivan", LastName = "Ivanov" };
            var project = new Project { Id = 1, Number = 1, Name = "P1", CreatorId = "u1", CreatedAt = DateTime.UtcNow };

            var t1 = new ProjectTask
            {
                Id = 10,
                Number = 1,
                Title = "T1",
                ProjectId = 1,
                ReporterId = "u1",
                AssigneeId = "u1",
                Type = BL.Enums.Task.Type.Task,
                Priority = Priority.Medium,
                Status = Status.ToDo,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(u1);
            await _context.Projects.AddAsync(project);
            await _context.Tasks.AddAsync(t1);
            await _context.SaveChangesAsync();

            var result = (await _repository.GetTasksByAssigneeAsync("u1")).ToList();

            var task = Assert.Single(result);
            Assert.Equal(10, task.Id);
            Assert.NotNull(task.Project);
            Assert.NotNull(task.Reporter);
        }

        [Fact]
        public async Task UpdateTaskAsync_UpdatesFieldsAndHandlesEmptyAssigneeId_WhenExists()
        {
            var task = new ProjectTask
            {
                Id = 10,
                Number = 1,
                Title = "Old Title",
                ProjectId = 1,
                ReporterId = "u1",
                AssigneeId = "u1",
                Type = BL.Enums.Task.Type.Task,
                Priority = Priority.Medium,
                Status = Status.ToDo,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            var dto = new UpdateTaskDto
            {
                Title = "New Title",
                Description = "New Desc",
                Type = BL.Enums.Task.Type.Bug,
                Priority = Priority.High,
                Status = Status.InProgress,
                Deadline = DateTime.UtcNow.AddDays(1),
                AssigneeId = "   "
            };

            var result = await _repository.UpdateTaskAsync(10, dto);

            Assert.True(result);
            var dbTask = await _context.Tasks.FindAsync(10);
            Assert.NotNull(dbTask);
            Assert.Equal("New Title", dbTask.Title);
            Assert.Equal(BL.Enums.Task.Type.Bug, dbTask.Type);
            Assert.Null(dbTask.AssigneeId);
        }

        [Fact]
        public async Task UpdateTaskAsync_ReturnsFalse_WhenDoesNotExist()
        {
            var dto = new UpdateTaskDto
            {
                Title = "Title",
                Type = BL.Enums.Task.Type.Task,
                Priority = Priority.Medium,
                Status = Status.ToDo
            };

            var result = await _repository.UpdateTaskAsync(99, dto);
            Assert.False(result);
        }

        [Fact]
        public async Task AddAsync_IncrementsNumberBasedOnTaskTypeSpecifically()
        {
            var t1 = new ProjectTask
            {
                Id = 10,
                Number = 5,
                Title = "T1",
                ProjectId = 1,
                ReporterId = "u1",
                Type = BL.Enums.Task.Type.Bug,
                Priority = Priority.Medium,
                Status = Status.ToDo,
                CreatedAt = DateTime.UtcNow
            };
            var t2 = new ProjectTask
            {
                Id = 11,
                Number = 12,
                Title = "T2",
                ProjectId = 1,
                ReporterId = "u1",
                Type = BL.Enums.Task.Type.Task,
                Priority = Priority.Medium,
                Status = Status.ToDo,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Tasks.AddRangeAsync(t1, t2);
            await _context.SaveChangesAsync();

            var newBug = new ProjectTask
            {
                Id = 12,
                Title = "New Bug",
                ProjectId = 1,
                ReporterId = "u1",
                Type = BL.Enums.Task.Type.Bug,
                Priority = Priority.Medium,
                Status = Status.ToDo,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(newBug);

            var dbBug = await _context.Tasks.FindAsync(12);
            Assert.NotNull(dbBug);
            Assert.Equal(6, dbBug.Number);
        }

        [Fact]
        public async Task UpdateStatusAsync_ReturnsTrueAndUpdatesStatus_WhenExists()
        {
            var task = new ProjectTask
            {
                Id = 10,
                Number = 1,
                Title = "T1",
                ProjectId = 1,
                ReporterId = "u1",
                Type = BL.Enums.Task.Type.Task,
                Priority = Priority.Medium,
                Status = Status.ToDo,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            var result = await _repository.UpdateStatusAsync(10, Status.Done);

            Assert.True(result);
            var dbTask = await _context.Tasks.FindAsync(10);
            Assert.NotNull(dbTask);
            Assert.Equal(Status.Done, dbTask.Status);
        }

        [Fact]
        public async Task UpdateStatusAsync_ReturnsFalse_WhenDoesNotExist()
        {
            var result = await _repository.UpdateStatusAsync(99, Status.Done);
            Assert.False(result);
        }

        [Fact]
        public async Task GetByIdAsync_IncludesDeepRelationsAndNestedUsers()
        {
            var user = new ApplicationUser { Id = "u1", FirstName = "Ivan", LastName = "Ivanov" };
            var project = new Project { Id = 1, Number = 1, Name = "P1", CreatorId = "u1", CreatedAt = DateTime.UtcNow };

            var task = new ProjectTask
            {
                Id = 10,
                Number = 1,
                Title = "T1",
                ProjectId = 1,
                ReporterId = "u1",
                AssigneeId = "u1",
                Type = BL.Enums.Task.Type.Task,
                Priority = Priority.Medium,
                Status = Status.ToDo,
                CreatedAt = DateTime.UtcNow
            };

            var comment = new Comment { Id = 1, Content = "C1", TaskId = 10, UserId = "u1", CreatedAt = DateTime.UtcNow };
            var log = new TimeLog { Id = 1, Hours = 2.5, Date = DateTime.UtcNow, TaskId = 10, UserId = "u1" };

            await _context.Users.AddAsync(user);
            await _context.Projects.AddAsync(project);
            await _context.Tasks.AddAsync(task);
            await _context.Comments.AddAsync(comment);
            await _context.TimeLogs.AddAsync(log);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(10);

            Assert.NotNull(result);
            Assert.NotNull(result.Project);
            Assert.NotNull(result.Assignee);
            Assert.NotNull(result.Reporter);

            var dbComment = Assert.Single(result.Comments);
            Assert.NotNull(dbComment.User);

            var dbLog = Assert.Single(result.TimeLogs);
            Assert.NotNull(dbLog.User);
        }
    }
}