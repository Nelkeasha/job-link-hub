using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.Public
{
    public class CandidatesModel : PageModel
    {
        private readonly IUserProfileService _profiles;
        public CandidatesModel(IUserProfileService profiles) { _profiles = profiles; }

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)] public string? Keyword { get; set; }
        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)] public string? Location { get; set; }

        public List<CandidateProfileDto> Candidates { get; set; } = new();

        public async Task OnGetAsync()
        {
            Candidates = (await _profiles.GetAllCandidatesAsync(Keyword, Location)).ToList();
        }
    }
}
