namespace ProjectManagementSystem.ViewModels.TimeLogs
{
    using System.ComponentModel.DataAnnotations;

    public class TimeLogViewModel
    {
        [Range(0, 99)]
        public int Days { get; set; }

        [Range(0, 23)]
        public int Hours { get; set; }

        [Range(0, 59)]
        public int Minutes { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string? Description { get; set; }
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
    }
}