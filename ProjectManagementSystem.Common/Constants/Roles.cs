namespace ProjectManagementSystem.Common.Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Member = "Member";

        public const string AdminOrManager = Admin + "," + Manager;

        public static readonly string[] All = [Admin, Manager, Member];
    }
}