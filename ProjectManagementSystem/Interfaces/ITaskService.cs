namespace ProjectManagementSystem.Interfaces
{
    using Enums.Task;
    using ViewModels.Tasks;

    public interface ITaskService
    {
        Task<(IEnumerable<TaskListViewModel> Tasks, string ProjectName)?> GetTasksByProjectAsync(int projectId);

        Task<TaskViewModel> GetTaskViewModelForCreateAsync();

        Task<EditTaskViewModel?> GetTaskForEditAsync(int id);

        Task<TaskDetailsViewModel?> GetTaskForDeleteAsync(int id);

        Task<TaskDetailsViewModel?> GetTaskDetailsAsync(int projectId, int id, string currentUserId);

        Task<(IEnumerable<TaskListViewModel> Tasks, string ProjectName, string ProjectTag)?> GetTasksForBoardAsync(int projectId);

        Task<bool> CreateTaskAsync(int projectId, TaskViewModel model, string currentUserId);

        Task<bool> UpdateTaskAsync(int id, EditTaskViewModel model, string userId);

        Task<bool> DeleteTaskAsync(int id, string userId);

        Task<bool> UpdateTaskStatusAsync(int id, Status status, string userId);
    }
}