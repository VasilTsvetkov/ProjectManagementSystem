namespace ProjectManagementSystem.Helpers
{
    using Constants;
    using Enums;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;

    public static class TaskHelper
    {
        public static string GetTypeIcon(TaskType type) => type switch
        {
            TaskType.Bug => TaskConstants.Icons[TaskConstants.BugPrefix],
            TaskType.Feature => TaskConstants.Icons[TaskConstants.FeaturePrefix],
            TaskType.Task => TaskConstants.Icons[TaskConstants.TaskPrefix],
            _ => ""
        };

        public static string GetDisplayName(this Enum value)
        {
            return value.GetType()
                .GetField(value.ToString())
                ?.GetCustomAttribute<DisplayAttribute>()
                ?.Name ?? value.ToString();
        }
    }
}