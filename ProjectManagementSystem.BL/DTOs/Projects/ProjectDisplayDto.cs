namespace ProjectManagementSystem.BL.DTOs.Projects
{
    public class ProjectDisplayDto
    {
        public int Id { get; init; }
        public int Number { get; init; }
        public required string Name { get; init; }
        public required string Tag { get; init; }
        public string? Description { get; init; }
        public required DateTime CreatedAt { get; init; }
    }
}