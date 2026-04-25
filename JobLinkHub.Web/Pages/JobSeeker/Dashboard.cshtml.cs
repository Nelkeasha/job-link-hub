using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "CANDIDATE")]
    public class DashboardModel : PageModel
    {
        private readonly IDashboardService _dashboard;
        private readonly IUserProfileService _profiles;

        public DashboardModel(IDashboardService dashboard, IUserProfileService profiles)
        {
            _dashboard = dashboard;
            _profiles = profiles;
        }

        public string FullName { get; set; } = "";
        public int TotalApplications { get; set; }
        public int ShortlistedCount { get; set; }
        public int SavedJobsCount { get; set; }
        public List<ApplicationDto> RecentApplications { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return;

            var profile = await _profiles.GetCandidateByUserIdAsync(userId);
            if (profile == null) return;

            FullName = $"{profile.FirstName} {profile.LastName}".Trim();

            var stats = await _dashboard.GetCandidateDashboardAsync(profile.Id);
            TotalApplications = stats.TotalApplications;
            ShortlistedCount = stats.ShortlistedApplications;
            SavedJobsCount = stats.SavedJobs;
            RecentApplications = stats.RecentApplications.Take(5).ToList();
        }
    }
}
