namespace ProjectManagementSystem.ViewModels.Comments
{
    public class CommentListViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool CanEdit { get; set; }
    }
}