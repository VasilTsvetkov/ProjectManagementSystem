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
            try
            {
                _logger.LogInformation("Creating comment for task {TaskId} by user {UserId}", model.TaskId, userId);
                var comment = new Comment
                {
                    Content = model.Content,
                    TaskId = model.TaskId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                await _commentRepository.AddAsync(comment);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                throw;
            }
        }

        public async Task<CommentViewModel?> GetCommentForEditAsync(int id, string userId)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(id);
                if (comment == null || comment.UserId != userId)
                {
                    _logger.LogWarning("Comment {Id} not found or unauthorized for user {UserId}", id, userId);
                    return null;
                }

                return new CommentViewModel
                {
                    Content = comment.Content,
                    TaskId = comment.TaskId,
                    ProjectId = comment.Task.ProjectId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching comment {Id} for edit", id);
                throw;
            }
        }

        public async Task<bool> UpdateCommentAsync(int id, CommentViewModel model, string userId)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(id);
                if (comment == null || comment.UserId != userId) return false;

                return await _commentRepository.UpdateCommentAsync(id, model.Content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment {Id}", id);
                throw;
            }
        }

        public async Task<(bool Success, int TaskId)?> DeleteCommentAsync(int id, string userId)
        {
            try
            {
                var comment = await _commentRepository.GetByIdAsync(id);
                if (comment == null || comment.UserId != userId) return null;

                var taskId = comment.TaskId;
                var deleted = await _commentRepository.DeleteAsync(id);
                return deleted ? (true, taskId) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment {Id}", id);
                throw;
            }
        }
    }
}