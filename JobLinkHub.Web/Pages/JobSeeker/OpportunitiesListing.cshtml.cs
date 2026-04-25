using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "CANDIDATE")]
    public class OpportunitiesListingModel : PageModel
    {
        private readonly IOpportunityService _opportunities;
        private readonly ISavedJobService _savedJobs;
        private readonly IUserProfileService _profiles;

        public OpportunitiesListingModel(
            IOpportunityService opportunities,
            ISavedJobService savedJobs,
            IUserProfileService profiles)
        {
            _opportunities = opportunities;
            _savedJobs = savedJobs;
            _profiles = profiles;
        }

        [BindProperty(SupportsGet = true)] public string? SearchQ { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterType { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterLocation { get; set; }

        public List<OpportunityDto> Opportunities { get; set; } = new();
        public HashSet<long> SavedIds { get; set; } = new();

        public async Task OnGetAsync()
        {
            Opportunities = (await _opportunities.GetAllAsync(SearchQ, FilterType, FilterLocation, "Active")).ToList();

            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdStr, out var userId))
            {
                var profile = await _profiles.GetCandidateByUserIdAsync(userId);
                if (profile != null)
                {
                    var saved = await _savedJobs.GetByJobSeekerAsync(profile.Id);
                    SavedIds = saved.Select(s => s.OpportunityId).ToHashSet();
                }
            }
        }

        public async Task<IActionResult> OnPostToggleSaveAsync(long opportunityId)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage();

            var profile = await _profiles.GetCandidateByUserIdAsync(userId);
            if (profile == null) return RedirectToPage();

            if (await _savedJobs.IsSavedAsync(profile.Id, opportunityId))
                await _savedJobs.UnsaveJobAsync(profile.Id, opportunityId);
            else
                await _savedJobs.SaveJobAsync(profile.Id, new SaveJobDto { OpportunityId = opportunityId });

            return RedirectToPage(new { SearchQ, FilterType, FilterLocation });
        }
    }
}
