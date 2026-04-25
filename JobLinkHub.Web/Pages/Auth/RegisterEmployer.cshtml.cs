using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Data;
using JobLinkHub.Data.Entities;
using JobLinkHub.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace JobLinkHub.Web.Pages.Auth
{
    public class RegisterEmployerModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signIn;
        private readonly AppDbContext _context;
        private readonly IEmailService _email;

        public RegisterEmployerModel(UserManager<User> userManager, SignInManager<User> signIn, AppDbContext context, IEmailService email)
        {
            _userManager = userManager;
            _signIn = signIn;
            _context = context;
            _email = email;
        }

        [BindProperty, Required(ErrorMessage = "First name is required"), StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "Last name is required"), StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "Email is required"), EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "Please confirm your password")]
        public string ConfirmPassword { get; set; } = "";

        [BindProperty, Required(ErrorMessage = "Company name is required"), StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        public string CompanyName { get; set; } = "";

        [BindProperty] public string? Industry { get; set; }
        [BindProperty] public string? Location { get; set; }
        [BindProperty] public string? Website { get; set; }
        [BindProperty] public string? CompanyDescription { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return Page();
            }

            var user = new User
            {
                UserName = Email,
                Email = Email,
                FirstName = FirstName,
                LastName = LastName,
                Role = "EMPLOYER",
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, Password);
            if (!result.Succeeded)
            {
                ErrorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
                return Page();
            }

            await _userManager.AddToRoleAsync(user, "EMPLOYER");

            var profile = new EmployerProfile
            {
                UserId = user.Id,
                CompanyName = CompanyName,
                Industry = Industry,
                Location = Location,
                Website = Website,
                CompanyDescription = CompanyDescription,
                CreatedAt = DateTime.UtcNow
            };
            _context.EmployerProfiles.Add(profile);
            await _context.SaveChangesAsync();

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);
            var verifyLink = $"{Request.Scheme}://{Request.Host}/Auth/VerifyEmail?userId={user.Id}&token={encodedToken}";
            await _email.SendEmailVerificationAsync(user.Email!, $"{user.FirstName} {user.LastName}", verifyLink);

            TempData["SuccessMessage"] = "Account created! Please check your email to verify your account before logging in.";
            return RedirectToPage("/Auth/Login");
        }
    }
}
