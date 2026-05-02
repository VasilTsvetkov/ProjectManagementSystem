namespace ProjectManagementSystem.ViewModels.Admin
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string CurrentRole { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }
}