using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "CANDIDATE")]
    public class SavedJobsModel : PageModel
    {
        private readonly ISavedJobService _savedJobs;
        private readonly IUserProfileService _profiles;

        public SavedJobsModel(ISavedJobService savedJobs, IUserProfileService profiles)
        {
            _savedJobs = savedJobs;
            _profiles = profiles;
        }

        public List<SavedJobDto> SavedJobs { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return;

            var profile = await _profiles.GetCandidateByUserIdAsync(userId);
            if (profile == null) return;

            SavedJobs = (await _savedJobs.GetByJobSeekerAsync(profile.Id)).ToList();
        }

        public async Task<IActionResult> OnPostRemoveAsync(long opportunityId)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage();

            var profile = await _profiles.GetCandidateByUserIdAsync(userId);
            if (profile != null)
                await _savedJobs.UnsaveJobAsync(profile.Id, opportunityId);

            return RedirectToPage();
        }
    }
}