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
        private readonly ICommentRepository _commentRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(ICommentRepository commentRepository, UserManager<ApplicationUser> userManager)
        {
            _commentRepository = commentRepository;
            _userManager = userManager;
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CommentViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Details", "Tasks", new { projectId = model.ProjectId, id = model.TaskId });

            var comment = new Comment
            {
                Content = model.Content,
                TaskId = model.TaskId,
                UserId = _userManager.GetUserId(User),
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddAsync(comment);
            return RedirectToAction("Details", "Tasks", new { projectId = model.ProjectId, id = model.TaskId });
        }

        [HttpGet("{id}/edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (comment.UserId != userId) return Forbid();

            var model = new CommentViewModel
            {
                Content = comment.Content,
                TaskId = comment.TaskId,
                ProjectId = comment.Task.ProjectId
            };

            return View(model);
        }

        [HttpPost("{id}/edit")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Edit(int id, CommentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (comment.UserId != userId) return Forbid();

            var updated = await _commentRepository.UpdateCommentAsync(id, model.Content);
            if (!updated) return NotFound();

            return RedirectToAction("Details", "Tasks", new { projectId = model.ProjectId, id = model.TaskId });
        }

        [HttpPost("{id}/delete")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, int projectId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (comment.UserId != userId) return Forbid();

            var taskId = comment.TaskId;
            var deleted = await _commentRepository.DeleteAsync(id);
            if (!deleted) return NotFound();

            return RedirectToAction("Details", "Tasks", new { projectId, id = taskId });
        }
    }
}