namespace ProjectManagementSystem.Constants
{
    public static class TaskConstants
    {
        public const string BugPrefix = "BUG";
        public const string TaskPrefix = "TSK";
        public const string FeaturePrefix = "FEAT";

        public static readonly string[] Colors =
        {
            "#3B82F6",
            "#8B5CF6",
            "#EC4899",
            "#F59E0B",
            "#10B981",
            "#06B6D4",
            "#6366F1",
            "#F97316",
            "#14B8A6",
            "#A855F7"
        };

        public static readonly Dictionary<string, string> Icons = new()
        {
            { BugPrefix, "🐞" },
            { TaskPrefix, "✅" },
            { FeaturePrefix, "⭐" }
        };
    }
}