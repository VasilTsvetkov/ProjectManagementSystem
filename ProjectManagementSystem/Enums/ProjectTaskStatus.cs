namespace ProjectManagementSystem.Enums
{
    using System.ComponentModel.DataAnnotations;

    public enum ProjectTaskStatus
    {
        [Display(Name = "To Do")]
        ToDo,
        [Display(Name = "In Progress")]
        InProgress,
        [Display(Name = "In Review")]
        InReview,
        Done
    }
}