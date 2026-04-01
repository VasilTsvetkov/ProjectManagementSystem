namespace ProjectManagementSystem.Repositories
{
    using Data;
    using Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext _context;
		private readonly DbSet<T> _dbSet;

		public Repository(ApplicationDbContext context)
		{
			_context = context;
			_dbSet = context.Set<T>();
		}

		public async Task<IEnumerable<T>> GetAllAsync()
			=> await _dbSet.ToListAsync();

		public async Task<T> GetByIdAsync(int id)
			=> await _dbSet.FindAsync(id);

		public async Task AddAsync(T entity)
		{
			await _dbSet.AddAsync(entity);
			await _context.SaveChangesAsync();
		}

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}