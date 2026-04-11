namespace ProjectManagementSystem.Constants
{
    using Enums;

    public static class Roles
    {
        public const string Admin = nameof(UserRole.Admin);
        public const string Manager = nameof(UserRole.Manager);
        public const string Member = nameof(UserRole.Member);

        public const string AdminOrManager = Admin + "," + Manager;
        public const string All = Admin + "," + Manager + "," + Member;
    }
}