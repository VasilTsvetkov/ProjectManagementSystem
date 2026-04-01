namespace ProjectManagementSystem.Repositories
{
    using Constants;
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
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

        public async Task<IEnumerable<ProjectTask>> GetTasksByAssigneeAsync(string userId)
            => await _context.Tasks
                .Include(t => t.Project)
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
            existing.AssigneeId = dto.AssigneeId;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task AddAsync(ProjectTask entity)
        {
            var prefix = entity.Type switch
            {
                TaskType.Bug => TaskConstants.BugPrefix,
                TaskType.Feature => TaskConstants.FeaturePrefix,
                TaskType.Task => TaskConstants.TaskPrefix,
                _ => TaskConstants.TaskPrefix
            };

            var count = await _context.Tasks
                .CountAsync(t => t.Type == entity.Type);

            entity.TaskNumber = count + 1;
            await _context.Tasks.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<ProjectTask?> GetTaskByIdAsync(int id)
            => await _context.Tasks
                .Include(t => t.Assignee)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);
    }
}