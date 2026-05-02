namespace ProjectManagementSystem.Services
{
    using Interfaces;
    using Models;
    using ViewModels.Comments;
    using Microsoft.Extensions.Logging;

    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger<CommentService> _logger;

        public CommentService(ICommentRepository commentRepository, ILogger<CommentService> logger)
        {
            _commentRepository = commentRepository;
            _logger = logger;
        }

        public async Task<bool> CreateCommentAsync(CommentViewModel model, string userId)
        {
            var comment = new Comment
            {
                Content = model.Content,
                TaskId = model.TaskId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddAsync(comment);

            _logger.LogInformation("User {UserId} added a comment to Task {TaskId}", userId, model.TaskId);

            return true;
        }

        public async Task<CommentViewModel?> GetCommentForEditAsync(int id, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null || comment.UserId != userId)
            {
                return null;
            }

            return new CommentViewModel
            {
                Content = comment.Content,
                TaskId = comment.TaskId,
                ProjectId = comment.Task.ProjectId
            };
        }

        public async Task<bool> UpdateCommentAsync(int id, CommentViewModel model, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null || comment.UserId != userId)
            {
                return false;
            }

            var result = await _commentRepository.UpdateCommentAsync(id, model.Content);

            if (result)
            {
                _logger.LogInformation("Comment {CommentId} updated by user {UserId}", id, userId);
            }

            return result;
        }

        public async Task<(bool Success, int TaskId)?> DeleteCommentAsync(int id, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null || comment.UserId != userId)
            {
                return null;
            }

            var taskId = comment.TaskId;
            var deleted = await _commentRepository.DeleteAsync(id);

            if (deleted)
            {
                _logger.LogInformation("Comment {CommentId} deleted from Task {TaskId} by user {UserId}", id, taskId, userId);
            }

            return deleted ? (true, taskId) : null;
        }
    }
}