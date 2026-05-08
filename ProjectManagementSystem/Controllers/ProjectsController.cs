namespace ProjectManagementSystem.Controllers
{
    using Constants;
    using Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using System.Security.Claims;
    using ViewModels.Projects;

    [Authorize]
    [Route("projects")]
    public class ProjectsController(IProjectService projectService, UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly IProjectService _projectService = projectService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Index()
        {
            var projects = await _projectService.GetAllProjectsAsync();
            return View(projects);
        }

        [HttpGet("create")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Create()
            => View();

        [HttpPost("create")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create(ProjectViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized();
            }

            await _projectService.CreateProjectAsync(model, userId);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id}/edit")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _projectService.GetProjectForEditAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost("{id}/edit")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Edit(int id, ProjectViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();

            var updated = await _projectService.UpdateProjectAsync(id, model, userId);
            if (!updated) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{id}/delete")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Unauthorized();

            var deleted = await _projectService.DeleteProjectAsync(id, userId);
            if (!deleted) return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}