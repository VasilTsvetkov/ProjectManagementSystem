namespace ProjectManagementSystem.ViewModels.TimeLogs
{
    using System.ComponentModel.DataAnnotations;

    public class TimeLogViewModel
    {
        public int Days { get; set; }

        public int Hours { get; set; }

        public int Minutes { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        public string? Description { get; set; }

        public required int TaskId { get; init; }

        public required int ProjectId { get; init; }
    }
}