namespace ProjectManagementSystem.Models
{
	public class Comment
	{
		public int Id { get; set; }
		public string Content { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public int TaskId { get; set; }
		public ProjectTask Task { get; set; } = null!;
		public string UserId { get; set; } = string.Empty;
		public ApplicationUser User { get; set; } = null!;
    }
}