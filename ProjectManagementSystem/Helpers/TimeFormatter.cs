namespace ProjectManagementSystem.Helpers
{
    public static class TimeFormatter
    {
        public static string Format(double hours)
        {
            var totalMinutes = (int)(hours * 60);
            var days = totalMinutes / 480;
            var remainingMinutes = totalMinutes % 480;
            var h = remainingMinutes / 60;
            var minutes = remainingMinutes % 60;

            var parts = new List<string>();
            if (days > 0) parts.Add($"{days}d");
            if (h > 0) parts.Add($"{h}h");
            if (minutes > 0) parts.Add($"{minutes}m");
            return parts.Any() ? string.Join(" ", parts) : "0m";
        }
    }
}