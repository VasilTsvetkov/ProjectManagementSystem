namespace ProjectManagementSystem.ViewModels.Tasks
{
    using Enums;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class TaskViewModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public TaskType Type { get; set; }

        [Required]
        public TaskPriority Priority { get; set; }

        public DateTime? Deadline { get; set; }

        public string? AssigneeId { get; set; }

        public IEnumerable<SelectListItem> Users { get; set; } = [];
    }
}