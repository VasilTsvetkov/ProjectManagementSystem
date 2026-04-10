namespace ProjectManagementSystem.Controllers
{
    using DTOs.Dashboard;
    using Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Models;
    using System.Globalization;
    using ViewModels.Dashboard;

    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ITimeLogRepository _timeLogRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ITimeLogRepository timeLogRepository,
            UserManager<ApplicationUser> userManager)
        {
            _timeLogRepository = timeLogRepository;
            _userManager = userManager;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Index(int? year, int? month)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var selectedYear = year ?? DateTime.Now.Year;
            var selectedMonth = month ?? DateTime.Now.Month;

            var stats = await _timeLogRepository.GetMonthlyStatsAsync(selectedYear, selectedMonth, currentUser.Id);
            var projectBreakdown = await _timeLogRepository.GetProjectBreakdownAsync(selectedYear, selectedMonth, currentUser.Id);

            var viewModel = new DashboardViewModel
            {
                Year = selectedYear,
                Month = selectedMonth,
                SelectedUserId = currentUser.Id,
                Stats = stats,
                ProjectBreakdown = projectBreakdown.ToList(),
                UserBreakdown = new List<UserTimeDto>(),
                AvailableMonths = GetMonthSelectList(),
                AvailableYears = GetYearSelectList(),
                AvailableUsers = new List<SelectListItem>(),
                CanViewAllUsers = false
            };

            return View(viewModel);
        }

        private List<SelectListItem> GetMonthSelectList()
        {
            return Enumerable.Range(1, 12)
                .Select(m => new SelectListItem
                {
                    Value = m.ToString(),
                    Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)
                })
                .ToList();
        }

        private List<SelectListItem> GetYearSelectList()
        {
            var currentYear = DateTime.Now.Year;
            return Enumerable.Range(currentYear - 2, 5)
                .Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString()
                })
                .ToList();
        }
    }
}