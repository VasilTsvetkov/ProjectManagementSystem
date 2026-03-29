namespace ProjectManagementSystem.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class CreateProjectViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
    }
}