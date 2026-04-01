namespace ProjectManagementSystem.Controllers
{
    using DTOs;
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
        private readonly IProjectRepository _projectRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController(IProjectRepository projectRepository, UserManager<ApplicationUser> userManager)
        {
            _projectRepository = projectRepository;
            _userManager = userManager;
        }

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var projects = await _projectRepository.GetProjectsByUserAsync(userId);

            var model = projects.Select(p => new ProjectListViewModel
            {
                Id = p.Id,
                Tag = p.Tag,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt
            });

            return View(model);
        }

        [HttpGet("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Create()
            => View();

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create(ProjectViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var project = new Project
            {
                Name = model.Name,
                Description = model.Description,
                CreatedByUserId = _userManager.GetUserId(User),
                CreatedAt = DateTime.UtcNow
            };

            await _projectRepository.AddAsync(project);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id}/edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null) return NotFound();

            var model = new ProjectViewModel
            {
                Name = project.Name,
                Description = project.Description
            };

            return View(model);
        }

        [HttpPost("{id}/edit")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Edit(int id, ProjectViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var updated = await _projectRepository.UpdateProjectAsync(id, model.Name, model.Description);

            if (!updated) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id}/delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null) return NotFound();

            var model = new ProjectDetailsViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt
            };

            return View(model);
        }

        [HttpPost("{id}/delete")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await _projectRepository.DeleteAsync(id);
            if (!deleted) return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}