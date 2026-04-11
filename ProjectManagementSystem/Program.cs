namespace ProjectManagementSystem
{
    using Extensions;
    using Serilog;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            ServiceCollectionExtensions.ConfigureSerilog();

            try
            {
                Log.Information("Starting Application");

                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog();

                builder.Services.AddDatabase(builder.Configuration);
                builder.Services.AddIdentityServices();
                builder.Services.AddRepositories();
                builder.Services.AddControllersWithViews();

                var app = builder.Build();

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
                Log.Fatal(ex, "Application failed to start");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}