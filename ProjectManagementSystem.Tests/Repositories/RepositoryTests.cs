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

    public class RepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Repository<Project> _repository;

        public RepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new Repository<Project>(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllEntities()
        {
            var projects = new List<Project>
            {
                new() { Id = 1, Number = 1, Name = "P1", CreatorId = "u1", CreatedAt = DateTime.UtcNow },
                new() { Id = 2, Number = 2, Name = "P2", CreatorId = "u1", CreatedAt = DateTime.UtcNow }
            };

            await _context.Projects.AddRangeAsync(projects);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsEntity_WhenExists()
        {
            var project = new Project { Id = 5, Number = 5, Name = "P5", CreatorId = "u1", CreatedAt = DateTime.UtcNow };
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(5);

            Assert.NotNull(result);
            Assert.Equal("P5", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenDoesNotExist()
        {
            var result = await _repository.GetByIdAsync(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_SavesEntityToDatabase()
        {
            var project = new Project { Id = 10, Number = 10, Name = "New Proj", CreatorId = "u1", CreatedAt = DateTime.UtcNow };

            await _repository.AddAsync(project);

            var dbProject = await _context.Projects.FindAsync(10);
            Assert.NotNull(dbProject);
            Assert.Equal("New Proj", dbProject.Name);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrueAndRemovesEntity_WhenExists()
        {
            var project = new Project { Id = 20, Number = 20, Name = "To Delete", CreatorId = "u1", CreatedAt = DateTime.UtcNow };
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            var result = await _repository.DeleteAsync(20);

            Assert.True(result);
            var dbProject = await _context.Projects.FindAsync(20);
            Assert.Null(dbProject);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenDoesNotExist()
        {
            var result = await _repository.DeleteAsync(999);

            Assert.False(result);
        }
    }
}