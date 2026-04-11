namespace ProjectManagementSystem.Helpers
{
    using Enums;

    public static class RoleHelper
    {
        public static string ToRoleName(this UserRole role) => role.ToString();

        public static UserRole[] GetAllRoles() => (UserRole[])Enum.GetValues(typeof(UserRole));

        public static string[] GetAllRoleNames() => GetAllRoles().Select(r => r.ToRoleName()).ToArray();
    }
}