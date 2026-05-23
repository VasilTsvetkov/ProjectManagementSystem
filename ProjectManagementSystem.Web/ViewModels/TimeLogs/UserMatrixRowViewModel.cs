namespace ProjectManagementSystem.Web.ViewModels.TimeLogs
{
    using System.Collections.Generic;
    using System.Linq;

    public class UserMatrixRowViewModel
    {
        public required string UserId { get; init; }

        public required string FullName { get; init; }

        public IReadOnlyDictionary<int, double> DailyHours { get; init; } = new Dictionary<int, double>();

        public double TotalHours => DailyHours.Values.Sum();
    }
}