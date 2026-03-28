namespace ProjectManagementSystem
{
	using Extensions;

	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddDatabase(builder.Configuration);
			builder.Services.AddIdentityServices();
			builder.Services.AddRepositories();
			builder.Services.AddControllersWithViews();

			var app = builder.Build();

			app.UseMiddlewarePipeline();
			app.Run();
		}
	}
}