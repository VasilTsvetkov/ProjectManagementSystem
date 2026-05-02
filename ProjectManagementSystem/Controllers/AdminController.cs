namespace ProjectManagementSystem.Controllers
{
    using Constants;
    using Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize(Roles = Roles.Admin)]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Index()
        {
            var users = await _adminService.GetAllUsersWithRolesAsync();
            return View(users);
        }

        [HttpPost("users/{userId}/change-role")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var (success, message) = await _adminService.ChangeUserRoleAsync(userId, newRole);

            if (success)
            {
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}