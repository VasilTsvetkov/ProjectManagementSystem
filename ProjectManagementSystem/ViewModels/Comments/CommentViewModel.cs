namespace ProjectManagementSystem.ViewModels.Comments
{
    using System.ComponentModel.DataAnnotations;

    public class CommentViewModel
    {
        [Required]
        public string Content { get; set; }
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
    }
}