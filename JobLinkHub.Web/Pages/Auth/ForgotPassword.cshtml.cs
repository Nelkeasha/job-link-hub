using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using JobLinkHub.Data.Entities;
using JobLinkHub.Services.Interfaces;

namespace JobLinkHub.Web.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _email;

        public ForgotPasswordModel(UserManager<User> userManager, IEmailService email)
        {
            _userManager = userManager;
            _email = email;
        }

        [BindProperty] public string Email { get; set; } = string.Empty;
        public bool EmailSent { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Please enter your email address.";
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = Uri.EscapeDataString(token);
                var encodedEmail = Uri.EscapeDataString(Email);
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var resetLink = $"{baseUrl}/Auth/ResetPassword?email={encodedEmail}&token={encodedToken}";

                await _email.SendPasswordResetAsync(
                    user.Email!,
                    $"{user.FirstName} {user.LastName}",
                    resetLink);
            }

            // Always show success to avoid revealing whether an email is registered
            EmailSent = true;
            return Page();
        }
    }
}