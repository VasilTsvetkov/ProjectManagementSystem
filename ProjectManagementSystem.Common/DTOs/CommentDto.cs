namespace ProjectManagementSystem.Common.DTOs
{
    public class CommentDto
    {
        public int Id { get; init; }
        public required string Content { get; init; }
        public required string AuthorName { get; init; }
        public DateTime CreatedAt { get; init; }
        public bool CanEdit { get; init; }
        public int TaskId { get; init; }
        public int ProjectId { get; init; }
    }
}