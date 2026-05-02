namespace ProjectManagementSystem.ViewModels.Projects
{
    public class ProjectDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}