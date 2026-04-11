namespace ProjectManagementSystem
{
    using Extensions;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
            app.Run();
        }
    }
}