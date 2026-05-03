namespace ProjectManagementSystem.Helpers
{
    using Constants;
    using System.Collections.Generic;

    public static class TimeFormatter
    {
        public static string Format(double totalHours)
        {
            var totalMinutes = (int)(totalHours * TimeConfig.MinutesInHour);

            var days = totalMinutes / (TimeConfig.WorkingHoursPerDay * TimeConfig.MinutesInHour);
            var remainingMinutesAfterDays = totalMinutes % (TimeConfig.WorkingHoursPerDay * TimeConfig.MinutesInHour);

            var h = remainingMinutesAfterDays / TimeConfig.MinutesInHour;
            var m = remainingMinutesAfterDays % TimeConfig.MinutesInHour;

            var parts = new List<string>();
            if (days > 0) parts.Add($"{days}d");
            if (h > 0) parts.Add($"{h}h");
            if (m > 0) parts.Add($"{m}m");

            return parts.Count > 0 ? string.Join(" ", parts) : "0m";
        }
    }
}