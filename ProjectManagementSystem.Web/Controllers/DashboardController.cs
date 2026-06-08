namespace ProjectManagementSystem.Web.Controllers
{
    using BL.Interfaces;
    using BL.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using ViewModels.Dashboard;

    [Authorize]
    [Route("dashboard")]
    public class DashboardController(
        IDashboardService dashboardService,
        UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly IDashboardService _dashboardService = dashboardService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        [HttpGet]
        [ProducesResponseType(typeof(DashboardViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Index(int? year, int? month)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var dto = await _dashboardService.GetDashboardDataAsync(year, month, userId);

            var currentYear = DateTime.UtcNow.Year;
            var availableMonths = Enumerable.Range(1, 12).Select(m => new SelectListItem
            {
                Value = m.ToString(),
                Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m),
                Selected = m == dto.Month
            }).ToList();

            var availableYears = Enumerable.Range(currentYear - 2, 5).Select(y => new SelectListItem
            {
                Value = y.ToString(),
                Text = y.ToString(),
                Selected = y == dto.Year
            }).ToList();

            var viewModel = new DashboardViewModel
            {
                Year = dto.Year,
                Month = dto.Month,
                SelectedUserId = dto.SelectedUserId,
                Stats = dto.Stats,
                ProjectBreakdown = dto.ProjectBreakdown,
                UserBreakdown = dto.UserBreakdown,
                AvailableMonths = availableMonths,
                AvailableYears = availableYears,
                AvailableUsers = [],
                CanViewAllUsers = dto.CanViewAllUsers
            };

            return View(viewModel);
        }
    }
}