namespace ProjectManagementSystem.Controllers
{
    using Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using ViewModels.Dashboard;

    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            IDashboardService dashboardService,
            UserManager<ApplicationUser> userManager)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
        }

        [HttpGet]
        [ProducesResponseType(typeof(DashboardViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Index(int? year, int? month)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var viewModel = await _dashboardService.GetDashboardDataAsync(year, month, userId);

            return View(viewModel);
        }
    }
}