namespace ProjectManagementSystem.Core.Models
{
    using Common.Constants;
    using Microsoft.AspNetCore.Identity;

    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string FullName => !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName)
            ? $"{FirstName} {LastName}"
            : UserName ?? MessageConstants.SystemUser;

        public virtual ICollection<ProjectTask> AssignedTasks { get; set; } = [];

        public virtual ICollection<ProjectTask> ReportedTasks { get; set; } = [];

        public virtual ICollection<Project> CreatedProjects { get; set; } = [];

        public virtual ICollection<TimeLog> TimeLogs { get; set; } = [];

        public virtual ICollection<Comment> Comments { get; set; } = [];
    }
}