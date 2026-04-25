using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.Public
{
    public class EmployersModel : PageModel
    {
        private readonly IUserProfileService _profiles;
        public EmployersModel(IUserProfileService profiles) { _profiles = profiles; }

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)] public string? Keyword { get; set; }
        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)] public string? Location { get; set; }

        public List<EmployerProfileDto> Employers { get; set; } = new();

        public async Task OnGetAsync()
        {
            Employers = (await _profiles.GetAllEmployersAsync(Keyword, Location)).ToList();
        }
    }
}
