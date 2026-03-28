namespace ProjectManagementSystem.Models
{
	public class TimeLog
	{
		public int Id { get; set; }
		public double Hours { get; set; }
		public DateTime Date { get; set; }
		public string Description { get; set; }
		public int TaskId { get; set; }
		public ProjectTask Task { get; set; }
		public string UserId { get; set; }
		public ApplicationUser User { get; set; }
	}
}