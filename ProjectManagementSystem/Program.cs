namespace ProjectManagementSystem
{
    using Extensions;
    using Middleware;
    using Serilog;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                builder.Host.ConfigureSerilog();

                builder.Services.AddDatabase(builder.Configuration);
                builder.Services.AddIdentityServices();
                builder.Services.AddRepositories();
                builder.Services.AddApplicationServices();
                builder.Services.AddControllersWithViews();

                var app = builder.Build();

                app.UseMiddleware<ExceptionHandlingMiddleware>();

                using (var scope = app.Services.CreateScope())
                {
                    await scope.ServiceProvider.SeedRolesAndAdminAsync();
                }

                app.UseMiddlewarePipeline();

                Log.Information("Application started successfully");

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly during startup");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}