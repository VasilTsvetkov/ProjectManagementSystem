namespace ProjectManagementSystem.Models
{
    public class Project
    {
        public int Id { get; set; }

        public int Number { get; set; }

        public required string Tag { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required string CreatorId { get; set; }

        public virtual ApplicationUser Creator { get; set; } = null!;

        public virtual ICollection<ProjectTask> Tasks { get; set; } = [];
    }
}