namespace ProjectManagementSystem.BL.Extensions
{
    using Constants;
    using Data;
    using Interfaces;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ProjectManagementSystem.BL.Models;
    using Repositories;
    using Services;
    using System;
    using System.Threading.Tasks;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration[AppSettingKeys.DefaultConnection];

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                    sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

            services.AddDatabaseDeveloperPageExceptionFilter();

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ITimeLogService, TimeLogService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IHomeService, HomeService>();
            services.AddScoped<IActivityService, ActivityService>();

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ITimeLogRepository, TimeLogRepository>();

            return services;
        }

        public static async Task SeedRolesAndAdminAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            await RoleSeeder.SeedRolesAsync(roleManager, userManager, configuration);
        }
    }
}