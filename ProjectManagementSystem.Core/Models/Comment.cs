namespace ProjectManagementSystem.Core.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public required string Content { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required int TaskId { get; set; }

        public virtual ProjectTask Task { get; set; } = null!;

        public required string UserId { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;
    }
}