namespace ProjectManagementSystem.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		public static IApplicationBuilder UseMiddlewarePipeline(this WebApplication app)
		{
			if (app.Environment.IsDevelopment())
			{
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();
			app.UseAuthorization();
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");
			app.MapRazorPages();

			return app;
		}
	}
}