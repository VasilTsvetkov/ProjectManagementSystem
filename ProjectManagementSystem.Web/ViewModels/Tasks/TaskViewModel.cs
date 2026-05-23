namespace ProjectManagementSystem.Web.ViewModels.Tasks
{
    using BL.Enums.Task;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System;
    using System.Collections.Generic;
    using Type = BL.Enums.Task.Type;

    public class TaskViewModel
    {
        public required string Title { get; set; }

        public string? Description { get; set; }

        public required Type Type { get; set; }

        public required Priority Priority { get; set; }

        public DateTime? Deadline { get; set; }

        public string? AssigneeId { get; set; }

        public IEnumerable<SelectListItem> Users { get; set; } = [];
    }
}