namespace ProjectManagementSystem.Web
{
    using BL.Extensions;
    using Extensions;
    using Middleware;
    using Serilog;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                builder.Host.ConfigureSerilog();

                builder.Services.AddDatabase(builder.Configuration);
                builder.Services.AddWebIdentityServices();
                builder.Services.AddRepositories();
                builder.Services.AddApplicationServices();
                builder.Services.AddControllersWithViews();
                builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

                var app = builder.Build();

                if (!app.Environment.IsDevelopment())
                {
                    app.UseHsts();
                }

                app.UseMiddleware<ExceptionHandlingMiddleware>();

                app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                await app.Services.SeedRolesAndAdminAsync();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                app.MapRazorPages();

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