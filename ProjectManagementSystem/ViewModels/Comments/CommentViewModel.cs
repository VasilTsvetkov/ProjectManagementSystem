namespace ProjectManagementSystem.ViewModels.Comments
{
    using System.ComponentModel.DataAnnotations;

    public class CommentViewModel
    {
        public required string Content { get; set; } = string.Empty;

        public required int TaskId { get; set; }

        public required int ProjectId { get; set; }
    }
}