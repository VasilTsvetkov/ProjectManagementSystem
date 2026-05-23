namespace ProjectManagementSystem.BL.Interfaces
{
    using DTOs;

    public interface ICommentService
    {
        Task<bool> CreateCommentAsync(CommentDto model, string userId);

        Task<CommentDto?> GetCommentForEditAsync(int id, string userId);

        Task<bool> UpdateCommentAsync(int id, CommentDto model, string userId);

        Task<(bool Success, int TaskId)?> DeleteCommentAsync(int id, string userId);
    }
}