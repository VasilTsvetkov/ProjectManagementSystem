namespace ProjectManagementSystem.ViewModels.Admin
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string CurrentRole { get; set; }
        public bool IsAdmin { get; set; }
    }
}