namespace ProjectManagementSystem.ViewModels.Tasks
{
    using Comments;
    using Enums;

    public class TaskDetailsViewModel
    {
        public int Id { get; set; }
        public string Tag { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskType Type { get; set; }
        public TaskPriority Priority { get; set; }
        public ProjectTaskStatus Status { get; set; }
        public DateTime? Deadline { get; set; }
        public string AssigneeEmail { get; set; }
        public int ProjectId { get; set; }
        public IEnumerable<CommentListViewModel> Comments { get; set; }
    }
}