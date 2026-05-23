namespace ProjectManagementSystem.Web.ViewModels.Tasks
{
    using BL.Enums.Task;

    public class EditTaskViewModel : TaskViewModel
    {
        public required Status Status { get; set; }
    }
}