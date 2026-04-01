namespace ProjectManagementSystem.ViewModels.Projects
{
    public class ProjectListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Tag { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}