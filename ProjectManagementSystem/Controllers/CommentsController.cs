namespace ProjectManagementSystem.Controllers
{
    using Interfaces;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using ViewModels.Comments;

    [Authorize]
    [Route("comments")]
    public class CommentsController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(ICommentService commentService, UserManager<ApplicationUser> userManager)
        {
            _commentService = commentService;
            _userManager = userManager;
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create(CommentViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Details", "Tasks", new { projectId = model.ProjectId, id = model.TaskId });

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized();
            }

            await _commentService.CreateCommentAsync(model, userId);

            return RedirectToAction("Details", "Tasks", new { projectId = model.ProjectId, id = model.TaskId });
        }

        [HttpGet("{id}/edit")]
        [ProducesResponseType(typeof(CommentViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized();
            }

            var model = await _commentService.GetCommentForEditAsync(id, userId);

            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost("{id}/edit")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(int id, CommentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized();
            }

            var success = await _commentService.UpdateCommentAsync(id, model, userId);

            if (!success) return NotFound();

            return RedirectToAction("Details", "Tasks", new { projectId = model.ProjectId, id = model.TaskId });
        }

        [HttpPost("{id}/delete")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int id, int projectId)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _commentService.DeleteCommentAsync(id, userId);

            if (result == null) return NotFound();

            return RedirectToAction("Details", "Tasks", new { projectId, id = result.Value.TaskId });
        }
    }
}