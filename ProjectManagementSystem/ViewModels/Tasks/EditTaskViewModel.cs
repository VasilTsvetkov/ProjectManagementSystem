namespace ProjectManagementSystem.ViewModels.Tasks
{
    using Enums;
    using System.ComponentModel.DataAnnotations;

    public class EditTaskViewModel : TaskViewModel
    {
        [Required]
        public ProjectTaskStatus Status { get; set; }
    }
}