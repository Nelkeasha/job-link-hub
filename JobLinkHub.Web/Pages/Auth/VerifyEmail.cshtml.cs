using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Data.Entities;

namespace JobLinkHub.Web.Pages.Auth
{
    public class VerifyEmailModel : PageModel
    {
        private readonly UserManager<User> _userManager;

        public VerifyEmailModel(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                ErrorMessage = "Invalid verification link.";
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ErrorMessage = "Invalid verification link.";
                return Page();
            }

            var decodedToken = Uri.UnescapeDataString(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                ErrorMessage = "This verification link is invalid or has expired. Please register again or contact support.";
                return Page();
            }

            Success = true;
            return Page();
        }
    }
}
