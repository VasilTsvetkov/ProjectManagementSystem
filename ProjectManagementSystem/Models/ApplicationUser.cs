namespace ProjectManagementSystem.Models
{
    using Microsoft.AspNetCore.Identity;

    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            AssignedTasks = new List<ProjectTask>();
            ReportedTasks = new List<ProjectTask>();
            CreatedProjects = new List<Project>();
            TimeLogs = new List<TimeLog>();
            Comments = new List<Comment>();
        }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public ICollection<ProjectTask> AssignedTasks { get; set; }
        public ICollection<ProjectTask> ReportedTasks { get; set; }
        public ICollection<Project> CreatedProjects { get; set; }
        public ICollection<TimeLog> TimeLogs { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}