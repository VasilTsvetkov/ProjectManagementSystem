namespace ProjectManagementSystem.ViewModels.Projects
{
    using System.ComponentModel.DataAnnotations;

    public class ProjectViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
    }
}