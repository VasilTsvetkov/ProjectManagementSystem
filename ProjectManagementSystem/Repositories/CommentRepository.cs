namespace ProjectManagementSystem.Repositories
{
    using Data;
    using Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class CommentRepository : Repository<Comment>, ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Comment?> GetByIdAsync(int id)
            => await _context.Comments
                .Include(c => c.Task)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<IEnumerable<Comment>> GetCommentsByTaskAsync(int taskId)
            => await _context.Comments
                .Include(c => c.User)
                .Where(c => c.TaskId == taskId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        public async Task<bool> UpdateCommentAsync(int id, string content)
        {
            var existing = await _context.Comments.FindAsync(id);
            if (existing == null) return false;

            existing.Content = content;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}