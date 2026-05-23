namespace ProjectManagementSystem.Web.ViewModels.Admin
{
    public class UserRoleViewModel
    {
        public required string UserId { get; init; }

        public required string Email { get; init; }

        public required string FullName { get; init; }

        public required string CurrentRole { get; init; }

        public bool IsAdmin { get; init; }
    }
}