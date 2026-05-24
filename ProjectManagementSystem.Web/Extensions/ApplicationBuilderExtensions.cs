namespace ProjectManagementSystem.Web.Extensions
{
    using BL.Data;
    using BL.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;

    public static class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddWebIdentityServices(this IServiceCollection services)
        {
            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            return services;
        }

        public static void ConfigureSerilog(this IHostBuilder host)
        {
            host.UseSerilog((context, loggerConfiguration) =>
            {
                loggerConfiguration
                    .Enrich.FromLogContext()
                    .Enrich.WithEnvironmentName()
                    .Enrich.WithThreadId();

                loggerConfiguration.ReadFrom.Configuration(context.Configuration);
            });
        }
    }
}