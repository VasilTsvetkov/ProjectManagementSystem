namespace ProjectManagementSystem.Models
{
	public class TimeLog
	{
		public int Id { get; set; }
		public double Hours { get; set; }
		public DateTime Date { get; set; }
		public string? Description { get; set; }
		public int TaskId { get; set; }
		public ProjectTask Task { get; set; } = null!;
		public string UserId { get; set; } = string.Empty;
		public ApplicationUser User { get; set; } = null!;
	}
}