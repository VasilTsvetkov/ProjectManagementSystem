namespace ProjectManagementSystem.Repositories
{
    using Data;
    using Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Models;
    using ProjectManagementSystem.Constants;

    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddAsync(Project entity)
        {
            var count = await _context.Projects.CountAsync();
            entity.Tag = $"{ProjectConstants.TagPrefix}-{count + 1}";
            await _context.Projects.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateProjectAsync(int id, string name, string description)
        {
            var existing = await _context.Projects.FindAsync(id);
            if (existing == null) return false;

            existing.Name = name;
            existing.Description = description;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}