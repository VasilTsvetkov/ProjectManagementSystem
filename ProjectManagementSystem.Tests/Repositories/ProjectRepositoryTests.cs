namespace ProjectManagementSystem.Tests.Repositories
{
    using BL.Data;
    using BL.Enums.Task;
    using BL.Models;
    using BL.Repositories;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;
    using Type = BL.Enums.Task.Type;

    public class ProjectRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ProjectRepository _repository;

        public ProjectRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new ProjectRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsProjectWithCreatorAndTasks_WhenExists()
        {
            var creator = new ApplicationUser { Id = "u1", FirstName = "Ivan", LastName = "Ivanov" };
            var project = new Project { Id = 1, Number = 1, Name = "P1", CreatorId = "u1", CreatedAt = DateTime.UtcNow };
            var task = new ProjectTask
            {
                Id = 10,
                Number = 1,
                Title = "T1",
                ProjectId = 1,
                ReporterId = "u1",
                Type = Type.Task,
                Priority = Priority.Medium,
                Status = Status.ToDo,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(creator);
            await _context.Projects.AddAsync(project);
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.NotNull(result.Creator);
            Assert.Single(result.Tasks);
            Assert.Equal("Ivan Ivanov", result.Creator.FullName);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllProjectsOrderedByCreatedAtDescending()
        {
            var creator = new ApplicationUser { Id = "u1", FirstName = "Ivan", LastName = "Ivanov" };
            var p1 = new Project { Id = 1, Number = 1, Name = "P1", CreatorId = "u1", CreatedAt = DateTime.UtcNow.AddDays(-2) };
            var p2 = new Project { Id = 2, Number = 2, Name = "P2", CreatorId = "u1", CreatedAt = DateTime.UtcNow };

            await _context.Users.AddAsync(creator);
            await _context.Projects.AddRangeAsync(p1, p2);
            await _context.SaveChangesAsync();

            var result = (await _repository.GetAllAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.Equal(1, result[1].Id);
            Assert.NotNull(result[0].Creator);
        }

        [Fact]
        public async Task AddAsync_SavesProjectSuccessfullyToDatabase()
        {
            var newProject = new Project { Id = 2, Number = 43, Name = "P2", CreatorId = "u1", CreatedAt = DateTime.UtcNow };

            await _repository.AddAsync(newProject);

            var dbProject = await _context.Projects.FindAsync(2);
            Assert.NotNull(dbProject);
            Assert.Equal("P2", dbProject.Name);
            Assert.Equal(43, dbProject.Number);
        }

        [Fact]
        public async Task AddAsync_PersistsProjectFieldsCorrectly()
        {
            var newProject = new Project { Id = 1, Number = 1, Name = "P1", CreatorId = "u1", CreatedAt = DateTime.UtcNow };

            await _repository.AddAsync(newProject);

            var dbProject = await _context.Projects.FindAsync(1);
            Assert.NotNull(dbProject);
            Assert.Equal(1, dbProject.Number);
            Assert.Equal("P1", dbProject.Name);
        }

        [Fact]
        public async Task UpdateProjectAsync_ReturnsTrueAndUpdatesFields_WhenExists()
        {
            var project = new Project { Id = 1, Number = 1, Name = "Old Name", Description = "Old Desc", CreatorId = "u1", CreatedAt = DateTime.UtcNow };
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            var result = await _repository.UpdateProjectAsync(1, "New Name", "New Desc");

            Assert.True(result);
            var dbProject = await _context.Projects.FindAsync(1);
            Assert.NotNull(dbProject);
            Assert.Equal("New Name", dbProject.Name);
            Assert.Equal("New Desc", dbProject.Description);
        }

        [Fact]
        public async Task UpdateProjectAsync_ReturnsFalse_WhenDoesNotExist()
        {
            var result = await _repository.UpdateProjectAsync(99, "Name", "Desc");

            Assert.False(result);
        }
    }
}