namespace ProjectManagementSystem.ViewModels.Tasks
{
    using Enums.Task;

    public class EditTaskViewModel : TaskViewModel
    {
        public required Status Status { get; set; }
    }
}