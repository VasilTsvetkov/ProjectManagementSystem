namespace ProjectManagementSystem.Web.Controllers
{
    using BL.Constants;
    using BL.Interfaces;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System.Linq;
    using System.Threading.Tasks;
    using ViewModels.Admin;

    [Authorize(Roles = Roles.Admin)]
    [Route("admin")]
    public class AdminController(IAdminService adminService) : Controller
    {
        private readonly IAdminService _adminService = adminService;

        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Index()
        {
            var dtos = await _adminService.GetAllUsersWithRolesAsync();

            var viewModels = dtos.Select(dto => new UserRoleViewModel
            {
                UserId = dto.UserId,
                Email = dto.Email,
                FullName = dto.FullName,
                CurrentRole = dto.CurrentRole,
                IsAdmin = dto.IsAdmin
            }).ToList();

            return View(viewModels);
        }

        [HttpPost("users/ChangeRole")]
        [IgnoreAntiforgeryToken]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var antiforgery = HttpContext.RequestServices.GetRequiredService<IAntiforgery>();
            try
            {
                await antiforgery.ValidateRequestAsync(HttpContext);
            }
            catch (AntiforgeryValidationException)
            {
                return BadRequest("Invalid security token.");
            }

            var (success, _) = await _adminService.ChangeUserRoleAsync(userId, newRole);

            if (!success)
            {
                return BadRequest("Could not update user role.");
            }

            return Ok();
        }
    }
}