namespace ProjectManagementSystem.Repositories
{
    using Data;
    using DTOs;
    using Enums;
    using Interfaces;
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class TaskRepository : Repository<ProjectTask>, ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectTask>> GetTasksByProjectAsync(int projectId)
            => await _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.Reporter)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

        public async Task<IEnumerable<ProjectTask>> GetTasksByAssigneeAsync(string userId)
            => await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.Reporter)
                .Where(t => t.AssigneeId == userId)
                .ToListAsync();

        public async Task<bool> UpdateTaskAsync(int id, UpdateTaskDto dto)
        {
            var existing = await _context.Tasks.FindAsync(id);

            if (existing == null) return false;

            existing.Title = dto.Title;
            existing.Description = dto.Description;
            existing.Type = dto.Type;
            existing.Priority = dto.Priority;
            existing.Status = dto.Status;
            existing.Deadline = dto.Deadline;
            existing.AssigneeId = string.IsNullOrWhiteSpace(dto.AssigneeId) ? null : dto.AssigneeId;

            await _context.SaveChangesAsync();
            return true;
        }

        public override async Task AddAsync(ProjectTask entity)
        {
            var maxNumber = await _context.Tasks
                .Where(t => t.Type == entity.Type)
                .MaxAsync(t => (int?)t.TaskNumber) ?? 0;

            entity.TaskNumber = maxNumber + 1;

            await _context.Tasks.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, ProjectTaskStatus status)
        {
            var existing = await _context.Tasks.FindAsync(id);
            if (existing == null) return false;

            existing.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public override async Task<ProjectTask?> GetByIdAsync(int id)
            => await _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.Reporter)
                .Include(t => t.Project)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .Include(t => t.TimeLogs)
                    .ThenInclude(tl => tl.User)
                .FirstOrDefaultAsync(t => t.Id == id);
    }
}