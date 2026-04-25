using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;
using System.ComponentModel.DataAnnotations;

namespace JobLinkHub.Web.Pages.Employer
{
    [Authorize(Roles = "EMPLOYER")]
    public class CompanyProfileModel : PageModel
    {
        private readonly IUserProfileService _profiles;
        public CompanyProfileModel(IUserProfileService profiles) { _profiles = profiles; }

        [BindProperty, Required(ErrorMessage = "Company name is required"), StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;
        [BindProperty] public string? CompanyType { get; set; }
        [BindProperty] public string? Industry { get; set; }
        [BindProperty] public string? Location { get; set; }
        [BindProperty] public string? Website { get; set; }
        [BindProperty] public string? CompanyDescription { get; set; }
        [BindProperty] public string? FirstName { get; set; }
        [BindProperty] public string? LastName { get; set; }
        [BindProperty] public string? PhoneNumber { get; set; }

        public string ContactEmail { get; set; } = string.Empty;
        public string? Message { get; set; }
        public bool Success { get; set; }

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadAsync();
                return Page();
            }

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return Page();

            try
            {
                await _profiles.UpdateEmployerAsync(userId, new UpdateEmployerProfileDto
                {
                    FirstName = FirstName ?? "",
                    LastName = LastName ?? "",
                    PhoneNumber = PhoneNumber,
                    CompanyName = CompanyName,
                    CompanyType = CompanyType,
                    Industry = Industry,
                    CompanyDescription = CompanyDescription,
                    Website = Website,
                    Location = Location
                });
                Success = true;
                Message = "Company profile saved successfully!";
            }
            catch (Exception ex) { Message = "Error: " + ex.Message; }

            await LoadAsync();
            return Page();
        }

        private async Task LoadAsync()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return;

            var profile = await _profiles.GetEmployerByUserIdAsync(userId);
            if (profile == null) return;

            FirstName = profile.FirstName;
            LastName = profile.LastName;
            PhoneNumber = profile.PhoneNumber;
            CompanyName = profile.CompanyName;
            CompanyType = profile.CompanyType;
            Industry = profile.Industry;
            Location = profile.Location;
            Website = profile.Website;
            CompanyDescription = profile.CompanyDescription;
            ContactEmail = profile.Email;
        }
    }
}