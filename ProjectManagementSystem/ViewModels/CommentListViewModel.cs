namespace ProjectManagementSystem.ViewModels.Comments
{
    public class CommentListViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string AuthorEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool CanEdit { get; set; }
    }
}