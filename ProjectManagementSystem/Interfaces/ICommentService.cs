namespace ProjectManagementSystem.Interfaces
{
    using ViewModels.Comments;

    public interface ICommentService
    {
        Task<bool> CreateCommentAsync(CommentViewModel model, string userId);
        Task<CommentViewModel?> GetCommentForEditAsync(int id, string userId);
        Task<bool> UpdateCommentAsync(int id, CommentViewModel model, string userId);
        Task<(bool Success, int TaskId)?> DeleteCommentAsync(int id, string userId);
    }
}