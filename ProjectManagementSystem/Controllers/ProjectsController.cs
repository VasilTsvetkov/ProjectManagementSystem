namespace ProjectManagementSystem.Controllers
{
    using Constants;
    using Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using ViewModels.Projects;

    [Authorize]
    [Route("projects")]
    public class ProjectsController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController(IProjectService projectService, UserManager<ApplicationUser> userManager)
        {
            _projectService = projectService;
            _userManager = userManager;
        }

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

            var updated = await _projectService.UpdateProjectAsync(id, model);
            if (!updated) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id}/delete")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _projectService.GetProjectForDeleteAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost("{id}/delete")]
        [Authorize(Roles = Roles.AdminOrManager)]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await _projectService.DeleteProjectAsync(id);
            if (!deleted) return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}