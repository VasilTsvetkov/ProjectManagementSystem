namespace ProjectManagementSystem.Interfaces
{
    using Enums;
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
        Task<bool> UpdateTaskAsync(int id, EditTaskViewModel model);
        Task<bool> DeleteTaskAsync(int id);
        Task<bool> UpdateTaskStatusAsync(int id, ProjectTaskStatus status);
    }
}