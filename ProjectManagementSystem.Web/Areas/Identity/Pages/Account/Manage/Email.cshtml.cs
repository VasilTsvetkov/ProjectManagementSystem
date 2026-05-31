namespace ProjectManagementSystem.Web.Areas.Identity.Pages.Account.Manage
{
    using BL.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.AspNetCore.WebUtilities;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using System.Text.Encodings.Web;

    public class EmailModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender) : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IEmailSender _emailSender = emailSender;

        public string Email { get; set; } = string.Empty;
        public bool IsEmailConfirmed { get; set; }

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            Email = await _userManager.GetEmailAsync(user) ?? string.Empty;
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            Input = new InputModel
            {
                NewEmail = Email,
            };

            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                Email = await _userManager.GetEmailAsync(user) ?? string.Empty;
                IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId, email = Input.NewEmail, code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.NewEmail,
                    "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

                StatusMessage = "Confirmation link to change email sent. Please check your email.";
                return RedirectToPage();
            }

            StatusMessage = "Your email is unchanged.";
            return RedirectToPage();
        }
    }
}