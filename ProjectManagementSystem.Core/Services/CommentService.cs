namespace ProjectManagementSystem.Core.Services
{
    using Common.Constants;
    using Common.DTOs;
    using Common.Enums;
    using Infrastructure.Models;
    using Interfaces;
    using Microsoft.Extensions.Logging;

    public class CommentService(
        ICommentRepository commentRepository,
        ITaskRepository taskRepository,
        IActivityService activityService,
        ILogger<CommentService> logger) : ICommentService
    {
        private readonly ICommentRepository _commentRepository = commentRepository;
        private readonly ITaskRepository _taskRepository = taskRepository;
        private readonly IActivityService _activityService = activityService;
        private readonly ILogger<CommentService> _logger = logger;

        public async Task<bool> CreateCommentAsync(CommentDto model, string userId)
        {
            var comment = new Comment
            {
                Content = model.Content,
                TaskId = model.TaskId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddAsync(comment);

            string taskTag = await GetTaskTagAsync(model.TaskId);

            await _activityService.LogAsync(
                userId,
                string.Format(MessageConstants.ActivityAddedComment, taskTag),
                ActivityType.CommentAction);

            _logger.LogInformation("User {UserId} added a comment to Task {TaskId}", userId, model.TaskId);

            return true;
        }

        public async Task<CommentDto?> GetCommentForEditAsync(int id, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null || comment.UserId != userId)
            {
                return null;
            }

            return new CommentDto
            {
                Content = comment.Content,
                TaskId = comment.TaskId,
                ProjectId = comment.Task.ProjectId
            };
        }

        public async Task<bool> UpdateCommentAsync(int id, CommentDto model, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);

            if (comment == null || comment.UserId != userId)
            {
                return false;
            }

            var result = await _commentRepository.UpdateCommentAsync(id, model.Content);

            if (result)
            {
                string taskTag = await GetTaskTagAsync(comment.TaskId);

                await _activityService.LogAsync(
                    userId,
                    string.Format(MessageConstants.ActivityUpdatedComment, taskTag),
                    ActivityType.CommentAction);

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
            string taskTag = await GetTaskTagAsync(taskId);

            var deleted = await _commentRepository.DeleteAsync(id);

            if (deleted)
            {
                await _activityService.LogAsync(
                    userId,
                    string.Format(MessageConstants.ActivityDeletedComment, taskTag),
                    ActivityType.CommentAction);

                _logger.LogInformation("Comment {CommentId} deleted from Task {TaskId} by user {UserId}", id, taskId, userId);
            }

            return deleted ? (true, taskId) : null;
        }

        private async Task<string> GetTaskTagAsync(int taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);

            if (task == null)
            {
                return MessageConstants.MissingTaskIdentifier;
            }

            return !string.IsNullOrWhiteSpace(task.Tag)
                ? task.Tag
                : MessageConstants.UntitledTask;
        }
    }
}