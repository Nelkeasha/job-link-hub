using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Data.Entities;

namespace JobLinkHub.Web.Pages.Auth
{
    public class VerifyOtpModel : PageModel
    {
        private readonly SignInManager<User> _signIn;
        private readonly UserManager<User> _userManager;

        public VerifyOtpModel(SignInManager<User> signIn, UserManager<User> userManager)
        {
            _signIn = signIn;
            _userManager = userManager;
        }

        [BindProperty] public string Code { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string MaskedEmail { get; set; } = string.Empty;

        public IActionResult OnGet()
        {
            var email = HttpContext.Session.GetString("OtpEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/Auth/Login");

            // Mask email: ne*****@gmail.com
            var parts = email.Split('@');
            MaskedEmail = parts[0][..Math.Min(2, parts[0].Length)] + "*****@" + parts[1];
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var storedOtp    = HttpContext.Session.GetString("OtpCode");
            var email        = HttpContext.Session.GetString("OtpEmail");
            var expiryStr    = HttpContext.Session.GetString("OtpExpiry");
            var rememberMe   = HttpContext.Session.GetString("OtpRememberMe") == "True";

            if (string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Session expired. Please log in again.";
                return RedirectToPage("/Auth/Login");
            }

            if (DateTime.TryParse(expiryStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiry)
                && DateTime.UtcNow > expiry)
            {
                ClearOtpSession();
                ErrorMessage = "This code has expired. Please log in again to receive a new one.";
                return Page();
            }

            if (Code.Trim() != storedOtp)
            {
                ErrorMessage = "Incorrect code. Please try again.";
                var parts = email.Split('@');
                MaskedEmail = parts[0][..Math.Min(2, parts[0].Length)] + "*****@" + parts[1];
                return Page();
            }

            ClearOtpSession();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return RedirectToPage("/Auth/Login");

            await _signIn.SignInAsync(user, isPersistent: rememberMe);

            return user.Role switch
            {
                "ADMIN"    => RedirectToPage("/Admin/AdminDashboard"),
                "EMPLOYER" => RedirectToPage("/Employer/EmployerDashboard"),
                _          => RedirectToPage("/JobSeeker/Dashboard")
            };
        }

        private void ClearOtpSession()
        {
            HttpContext.Session.Remove("OtpCode");
            HttpContext.Session.Remove("OtpEmail");
            HttpContext.Session.Remove("OtpExpiry");
            HttpContext.Session.Remove("OtpRememberMe");
        }
    }
}
