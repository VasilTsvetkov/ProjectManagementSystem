namespace ProjectManagementSystem.Tests.Repositories
{
    using BL.Data;
    using BL.Models;
    using BL.Repositories;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class CommentRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CommentRepository _repository;

        public CommentRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new CommentRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCommentWithTaskAndUser_WhenExists()
        {
            var user = new ApplicationUser { Id = "u1", FirstName = "Ivan", LastName = "Ivanov" };
            var task = new ProjectTask
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
            var comment = new Comment { Id = 100, Content = "Content 100", TaskId = 10, UserId = "u1", CreatedAt = DateTime.UtcNow };

            await _context.Users.AddAsync(user);
            await _context.Tasks.AddAsync(task);
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(100);

            Assert.NotNull(result);
            Assert.NotNull(result.Task);
            Assert.NotNull(result.User);
            Assert.Equal("Content 100", result.Content);
        }

        [Fact]
        public async Task GetCommentsByTaskAsync_ReturnsCommentsOrderedByCreatedAtDescending()
        {
            var user = new ApplicationUser { Id = "u1", FirstName = "Ivan", LastName = "Ivanov" };
            var comment1 = new Comment { Id = 1, Content = "First", TaskId = 10, UserId = "u1", CreatedAt = DateTime.UtcNow.AddMinutes(-5) };
            var comment2 = new Comment { Id = 2, Content = "Second", TaskId = 10, UserId = "u1", CreatedAt = DateTime.UtcNow };
            var comment3 = new Comment { Id = 3, Content = "Other Task", TaskId = 20, UserId = "u1", CreatedAt = DateTime.UtcNow };

            await _context.Users.AddAsync(user);
            await _context.Comments.AddRangeAsync(comment1, comment2, comment3);
            await _context.SaveChangesAsync();

            var result = (await _repository.GetCommentsByTaskAsync(10)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.Equal(1, result[1].Id);
            Assert.NotNull(result[0].User);
        }

        [Fact]
        public async Task UpdateCommentAsync_ReturnsTrueAndUpdatesContent_WhenExists()
        {
            var comment = new Comment { Id = 1, Content = "Old Content", TaskId = 10, UserId = "u1", CreatedAt = DateTime.UtcNow };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var result = await _repository.UpdateCommentAsync(1, "New Content");

            Assert.True(result);
            var dbComment = await _context.Comments.FindAsync(1);
            Assert.NotNull(dbComment);
            Assert.Equal("New Content", dbComment.Content);
        }

        [Fact]
        public async Task UpdateCommentAsync_ReturnsFalse_WhenDoesNotExist()
        {
            var result = await _repository.UpdateCommentAsync(999, "Updated");
            Assert.False(result);
        }
    }
}