namespace ProjectManagementSystem
{
    using Extensions;
    using Serilog;

    public class Program
    {
        private static readonly ILogger _logger = Log.Logger;

        public static async Task Main(string[] args)
        {
            ServiceCollectionExtensions.ConfigureSerilog();

            try
            {
                _logger.Information("Starting Application");

                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog();

                builder.Services.AddDatabase(builder.Configuration);
                builder.Services.AddIdentityServices();
                builder.Services.AddRepositories();
                builder.Services.AddApplicationServices();
                builder.Services.AddControllersWithViews();

                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    await scope.ServiceProvider.SeedRolesAndAdminAsync();
                }

                app.UseMiddlewarePipeline();

                _logger.Information("Application started successfully");
                app.Run();
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Application failed to start");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}