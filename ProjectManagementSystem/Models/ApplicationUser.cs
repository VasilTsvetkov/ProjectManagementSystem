namespace ProjectManagementSystem.Models
{
	using Microsoft.AspNetCore.Identity;

	public class ApplicationUser : IdentityUser
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public ICollection<ProjectTask> Tasks { get; set; }
		public ICollection<TimeLog> TimeLogs { get; set; }
		public ICollection<Comment> Comments { get; set; }
	}
}