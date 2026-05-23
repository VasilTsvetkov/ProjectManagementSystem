namespace ProjectManagementSystem.Web.Controllers
{
    using BL.Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Diagnostics;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using ViewModels.Home;

    [Authorize]
    public class HomeController(IHomeService homeService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var dto = await homeService.GetHomeIndexDataAsync(userId);

            var model = new IndexViewModel
            {
                MyPendingTasksCount = dto.MyPendingTasksCount,
                OverdueTasksCount = dto.OverdueTasksCount,
                UrgentTasks = dto.UrgentTasks,
                RecentActivities = dto.RecentActivities
            };

            return View(model);
        }

        [AllowAnonymous]
        [Route("Home/Error/{statusCode?}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode
            });
        }
    }
}