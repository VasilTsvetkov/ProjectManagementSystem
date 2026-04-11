namespace ProjectManagementSystem.Data
{
	using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
	using Microsoft.EntityFrameworkCore;
	using Models;
    using System.Reflection.Emit;

	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<Project> Projects { get; set; }
		public DbSet<ProjectTask> Tasks { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<TimeLog> TimeLogs { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<Comment>()
				.HasOne(c => c.Task)
				.WithMany(t => t.Comments)
				.HasForeignKey(c => c.TaskId)
				.OnDelete(DeleteBehavior.NoAction);

			builder.Entity<Comment>()
				.HasOne(c => c.User)
				.WithMany(u => u.Comments)
				.HasForeignKey(c => c.UserId)
				.OnDelete(DeleteBehavior.NoAction);

			builder.Entity<TimeLog>()
				.HasOne(t => t.Task)
				.WithMany(t => t.TimeLogs)
				.HasForeignKey(t => t.TaskId)
				.OnDelete(DeleteBehavior.NoAction);

			builder.Entity<TimeLog>()
				.HasOne(t => t.User)
				.WithMany(u => u.TimeLogs)
				.HasForeignKey(t => t.UserId)
				.OnDelete(DeleteBehavior.NoAction);

			builder.Entity<ProjectTask>()
				.HasOne(t => t.Assignee)
				.WithMany(u => u.Tasks)
				.HasForeignKey(t => t.AssigneeId)
				.OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ProjectTask>()
               .HasMany(t => t.Comments)
               .WithOne(c => c.Task)
               .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProjectTask>()
                .HasMany(t => t.TimeLogs)
                .WithOne(tl => tl.Task)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Project>()
                .HasMany(p => p.Tasks)
                .WithOne(t => t.Project)
                .OnDelete(DeleteBehavior.Cascade);
        }
	}
}