namespace ProjectManagementSystem.BL.Interfaces
{
    using DTOs.Tasks;
    using Enums.Task;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITaskService
    {
        Task<(IEnumerable<TaskListDto> Tasks, string ProjectName)?> GetTasksByProjectAsync(int projectId);

        Task<TaskDto> GetTaskForCreateAsync();

        Task<TaskDto?> GetTaskForEditAsync(int id);

        Task<TaskDetailsDto?> GetTaskForDeleteAsync(int id);

        Task<TaskDetailsDto?> GetTaskDetailsAsync(int projectId, int id, string currentUserId);

        Task<(IEnumerable<TaskListDto> Tasks, string ProjectName, string ProjectTag)?> GetTasksForBoardAsync(int projectId);

        Task<bool> CreateTaskAsync(int projectId, TaskDto model, string currentUserId);

        Task<bool> UpdateTaskAsync(int id, TaskDto model, string userId);

        Task<bool> DeleteTaskAsync(int id, string userId);

        Task<bool> UpdateTaskStatusAsync(int id, Status status, string userId);
    }
}