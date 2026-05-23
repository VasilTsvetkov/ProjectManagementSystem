namespace ProjectManagementSystem.BL.Helpers
{
    using Constants;
    using Enums.Task;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;

    public static class TaskHelper
    {
        public static string GetTypeIcon(Type type) => type switch
        {
            Type.Bug => TaskConstants.Icons[TaskConstants.BugPrefix],
            Type.Feature => TaskConstants.Icons[TaskConstants.FeaturePrefix],
            Type.Task => TaskConstants.Icons[TaskConstants.TaskPrefix],
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