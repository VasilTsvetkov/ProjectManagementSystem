namespace ProjectManagementSystem.ViewModels.Projects
{
    public class ProjectDisplayViewModel
    {
        public int Id { get; init; }

        public required string Name { get; init; }

        public required string Tag { get; init; }

        public string? Description { get; init; }

        public required DateTime CreatedAt { get; init; }
    }
}