namespace ProjectManagementSystem.Repositories
{
    using Data;
    using Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetProjectsByUserAsync(string userId)
            => await _context.Projects
                .Where(p => p.CreatedByUserId == userId)
                .ToListAsync();

        public async Task<bool> UpdateProjectAsync(int id, string name, string description)
        {
            var existing = await _context.Projects.FindAsync(id);
            if (existing == null) return false;

            existing.Name = name;
            existing.Description = description;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsWithNameAsync(string name, int? excludeId = null)
            => await _context.Projects
                .AnyAsync(p => p.Name == name && (!excludeId.HasValue || p.Id != excludeId.Value));
    }
}