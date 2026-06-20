namespace ProjectManagementSystem.Web.Controllers
{
    using BL.Constants;
    using BL.DTOs;
    using BL.Interfaces;
    using BL.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using ViewModels.Comments;

    [Authorize]
    [Route("comments")]
    public class CommentsController(ICommentService commentService, UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly ICommentService _commentService = commentService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create(CommentViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(TaskConstants.DetailsAction, TaskConstants.Controller, new { projectId = model.ProjectId, id = model.TaskId });

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var dto = new CommentDto
            {
                Content = model.Content,
                TaskId = model.TaskId,
                ProjectId = model.ProjectId,
                AuthorName = string.Empty
            };

            await _commentService.CreateCommentAsync(dto, userId);

            return RedirectToAction(TaskConstants.DetailsAction, TaskConstants.Controller, new { projectId = model.ProjectId, id = model.TaskId });
        }

        [HttpGet("{id}/edit")]
        [ProducesResponseType(typeof(CommentViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var dto = await _commentService.GetCommentForEditAsync(id, userId);
            if (dto == null) return NotFound();

            var model = new CommentViewModel
            {
                Content = dto.Content,
                TaskId = dto.TaskId,
                ProjectId = dto.ProjectId
            };

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
            if (userId == null) return Unauthorized();

            var dto = new CommentDto
            {
                Id = id,
                Content = model.Content,
                TaskId = model.TaskId,
                ProjectId = model.ProjectId,
                AuthorName = string.Empty
            };

            var success = await _commentService.UpdateCommentAsync(id, dto, userId);
            if (!success) return NotFound();

            return RedirectToAction(TaskConstants.DetailsAction, TaskConstants.Controller, new { projectId = model.ProjectId, id = model.TaskId });
        }

        [HttpPost("{id}/delete")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, int projectId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var result = await _commentService.DeleteCommentAsync(id, userId);
            if (result == null) return NotFound();

            return RedirectToAction(TaskConstants.DetailsAction, TaskConstants.Controller, new { projectId, id = result.Value.TaskId });
        }
    }
}