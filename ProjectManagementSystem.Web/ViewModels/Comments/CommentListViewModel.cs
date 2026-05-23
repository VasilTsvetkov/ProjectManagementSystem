namespace ProjectManagementSystem.Web.ViewModels.Comments
{
    public class CommentListViewModel
    {
        public int Id { get; init; }

        public required string Content { get; init; }

        public required string AuthorName { get; init; }

        public required DateTime CreatedAt { get; init; }

        public bool CanEdit { get; init; }
    }
}