namespace ProjectManagementSystem.Models
{
    public class TimeLog
    {
        public int Id { get; set; }

        public required double Hours { get; set; }

        public required DateTime Date { get; set; }

        public string? Description { get; set; }

        public required int TaskId { get; set; }

        public virtual ProjectTask Task { get; set; } = null!;

        public required string UserId { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;
    }
}