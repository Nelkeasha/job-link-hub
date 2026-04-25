using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using JobLinkHub.Data.Entities;

namespace JobLinkHub.Web.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public ResetPasswordModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)] public string Email { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)] public string Token { get; set; } = string.Empty;
        [BindProperty] public string NewPassword { get; set; } = string.Empty;
        [BindProperty] public string ConfirmPassword { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public bool InvalidLink { get; set; }

        public void OnGet()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Token))
                InvalidLink = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Token))
            {
                ErrorMessage = "Invalid or expired reset link.";
                return Page();
            }

            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return Page();
            }

            if (NewPassword.Length < 8)
            {
                ErrorMessage = "Password must be at least 8 characters.";
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                ErrorMessage = "Invalid or expired reset link.";
                return Page();
            }

            var decodedToken = Uri.UnescapeDataString(Token);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, NewPassword);

            if (!result.Succeeded)
            {
                ErrorMessage = result.Errors.FirstOrDefault()?.Description ?? "Password reset failed.";
                return Page();
            }

            TempData["SuccessMessage"] = "Password reset successfully. Please log in with your new password.";
            return RedirectToPage("/Auth/Login");
        }
    }
}