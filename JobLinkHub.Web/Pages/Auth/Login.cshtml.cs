using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using JobLinkHub.Data.Entities;
using JobLinkHub.Services.Interfaces;

namespace JobLinkHub.Web.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signIn;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _email;

        public LoginModel(SignInManager<User> signIn, UserManager<User> userManager, IEmailService email)
        {
            _signIn = signIn;
            _userManager = userManager;
            _email = email;
        }

        [BindProperty] public string Email { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";
        [BindProperty] public bool RememberMe { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Email and password are required.";
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null || !user.IsActive)
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            if (!user.EmailConfirmed)
            {
                ErrorMessage = "Please verify your email before logging in. Check your inbox for the verification link.";
                return Page();
            }

            var passwordOk = await _userManager.CheckPasswordAsync(user, Password);
            if (!passwordOk)
            {
                ErrorMessage = "Invalid email or password.";
                return Page();
            }

            // Admins sign in directly — no OTP required
            if (user.Role == "ADMIN")
            {
                await _signIn.SignInAsync(user, isPersistent: RememberMe);
                return RedirectToPage("/Admin/AdminDashboard");
            }

            // Generate 6-digit OTP, store in session, send via email
            var otp = Random.Shared.Next(100000, 999999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(10).ToString("o");

            HttpContext.Session.SetString("OtpCode", otp);
            HttpContext.Session.SetString("OtpEmail", user.Email!);
            HttpContext.Session.SetString("OtpExpiry", expiry);
            HttpContext.Session.SetString("OtpRememberMe", RememberMe.ToString());

            await _email.SendLoginOtpAsync(user.Email!, $"{user.FirstName} {user.LastName}", otp);

            return RedirectToPage("/Auth/VerifyOtp");
        }
    }
}