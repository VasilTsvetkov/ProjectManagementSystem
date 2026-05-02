namespace ProjectManagementSystem.Helpers
{
    using Models;

    public static class UserDisplayNameHelper
    {
        public static string GetFullName(ApplicationUser? user)
        {
            if (user == null) return "Unassigned";

            var firstName = user.FirstName ?? string.Empty;
            var lastName = user.LastName ?? string.Empty;

            var fullName = $"{firstName} {lastName}".Trim();

            return string.IsNullOrWhiteSpace(fullName) ? "Unknown User" : fullName;
        }
    }
}