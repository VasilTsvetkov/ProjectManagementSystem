namespace ProjectManagementSystem.BL.Enums.Task
{
    using System.ComponentModel.DataAnnotations;

    public enum Status
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