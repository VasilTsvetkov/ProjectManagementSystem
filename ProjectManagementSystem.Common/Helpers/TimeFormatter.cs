namespace ProjectManagementSystem.Common.Helpers
{
	using Constants;
	using System;
	using System.Collections.Generic;

	public static class TimeFormatter
	{
		public static string Format(double totalHours)
		{
			if (totalHours <= 0.001) return "0h";

			var totalMinutes = (int)Math.Round(totalHours * TimeConfig.MinutesInHour);

			var days = totalMinutes / (TimeConfig.WorkingHoursPerDay * TimeConfig.MinutesInHour);
			var remainingAfterDays = totalMinutes % (TimeConfig.WorkingHoursPerDay * TimeConfig.MinutesInHour);

			var h = remainingAfterDays / TimeConfig.MinutesInHour;
			var m = remainingAfterDays % TimeConfig.MinutesInHour;

			var parts = new List<string>();
			if (days > 0) parts.Add($"{days}d");
			if (h > 0) parts.Add($"{h}h");
			if (m > 0) parts.Add($"{m}m");

			return parts.Count > 0 ? string.Join(" ", parts) : ".";
		}
	}
}