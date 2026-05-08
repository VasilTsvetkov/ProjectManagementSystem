namespace ProjectManagementSystem.ViewModels.TimeLogs
{
    using System.ComponentModel.DataAnnotations;

    public class TimeLogViewModel
    {
        [Range(0, 8, ErrorMessage = "You can log a maximum of 1 day")]
        public int Days { get; set; }

		[Range(0, 8, ErrorMessage = "Hours must be between 0 and 8")]
		public int Hours { get; set; }

		[Range(0, 59, ErrorMessage = "Minutes must be between 0 and 59")]
		public int Minutes { get; set; }

		[Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        public string? Description { get; set; }

        public int TaskId { get; init; }

        public int ProjectId { get; init; }
    }
}