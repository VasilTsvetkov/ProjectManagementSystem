namespace ProjectManagementSystem.ViewModels.Projects
{
    using System.ComponentModel.DataAnnotations;

    public class ProjectViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;
    }
}