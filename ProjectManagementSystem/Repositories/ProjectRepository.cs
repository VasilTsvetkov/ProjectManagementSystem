namespace ProjectManagementSystem.Repositories
{
    using Constants;
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

        public override async Task<Project?> GetByIdAsync(int id)
            => await _context.Projects
                .Include(p => p.Creator)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);

        public override async Task<IEnumerable<Project>> GetAllAsync()
            => await _context.Projects
                .Include(p => p.Creator)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

        public override async Task AddAsync(Project entity)
        {
            var maxNumber = await _context.Projects
                .MaxAsync(p => (int?)p.Number) ?? 0;

            entity.Number = maxNumber + 1;
            entity.Tag = $"{ProjectConstants.TagPrefix}-{entity.Number}";

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