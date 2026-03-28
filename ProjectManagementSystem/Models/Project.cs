namespace ProjectManagementSystem.Models
{
	public class Project
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public DateTime CreatedAt { get; set; }
		public string CreatedByUserId { get; set; }
		public ApplicationUser CreatedBy { get; set; }
		public ICollection<ProjectTask> Tasks { get; set; }
	}
}