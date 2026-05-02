namespace ProjectManagementSystem.Models
{
    public class Project
    {
        public Project()
        {
            Tasks = new List<ProjectTask>();
        }

        public int Id { get; set; }
        public int Number { get; set; }
        public string Tag { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatorId { get; set; } = string.Empty;
        public ApplicationUser? Creator { get; set; }
        public ICollection<ProjectTask> Tasks { get; set; }
    }
}