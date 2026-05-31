namespace ProjectManagementSystem.Web.Areas.Identity.Pages.Account.Manage
{
    using BL.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using System.ComponentModel.DataAnnotations;

    public class IndexModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;

        public string Username { get; set; } = string.Empty;

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            Username = await _userManager.GetUserNameAsync(user) ?? string.Empty;
            Input = new InputModel
            {
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user) ?? string.Empty
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                Username = await _userManager.GetUserNameAsync(user) ?? string.Empty;
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}