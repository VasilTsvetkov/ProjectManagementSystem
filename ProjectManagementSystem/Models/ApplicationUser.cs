namespace ProjectManagementSystem.Models
{
    using Microsoft.AspNetCore.Identity;

    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            AssignedTasks = [];
            ReportedTasks = [];
            CreatedProjects = [];
            TimeLogs = [];
            Comments = [];
        }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string FullName => !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName)
            ? $"{FirstName} {LastName}"
            : UserName ?? "System User";

        public ICollection<ProjectTask> AssignedTasks { get; set; }
        public ICollection<ProjectTask> ReportedTasks { get; set; }
        public ICollection<Project> CreatedProjects { get; set; }
        public ICollection<TimeLog> TimeLogs { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}