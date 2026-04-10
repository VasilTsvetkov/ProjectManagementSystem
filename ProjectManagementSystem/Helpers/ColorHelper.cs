namespace ProjectManagementSystem.Helpers
{
    using Constants;

    public static class ColorHelper
    {
        public static string GetProjectColor(string tag)
        {
            var hash = tag.Aggregate(0, (acc, c) => acc + c);
            return ProjectConstants.Colors[hash % ProjectConstants.Colors.Length];
        }

        public static string GetTaskColor(string tag)
        {
            var hash = tag.Aggregate(0, (acc, c) => acc + c);
            return TaskConstants.Colors[hash % TaskConstants.Colors.Length];
        }
    }
}