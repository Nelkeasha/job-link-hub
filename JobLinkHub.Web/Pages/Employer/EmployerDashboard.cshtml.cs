using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.Employer
{
    [Authorize(Roles = "EMPLOYER")]
    public class EmployerDashboardModel : PageModel
    {
        private readonly IDashboardService _dashboard;
        private readonly IUserProfileService _profiles;

        public EmployerDashboardModel(IDashboardService dashboard, IUserProfileService profiles)
        {
            _dashboard = dashboard;
            _profiles = profiles;
        }

        public string CompanyName { get; set; } = string.Empty;
        public int ActiveOpportunitiesCount { get; set; }
        public int TotalApplicantsCount { get; set; }
        public int TotalViews { get; set; }
        public int PendingReviewsCount { get; set; }
        public int ShortlistedCandidatesCount { get; set; }
        public int ApplicationsReceivedCount { get; set; }
        public int CandidatesShortlistedMetric { get; set; }
        public int RolesClosingSoonCount { get; set; }
        public List<OpportunityDto> RecentOpportunities { get; set; } = new();
        public List<ApplicationDto> RecentApplicants { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return;

            var profile = await _profiles.GetEmployerByUserIdAsync(userId);
            if (profile == null) return;

            CompanyName = profile.CompanyName;
            var stats = await _dashboard.GetEmployerDashboardAsync(profile.Id);
            ActiveOpportunitiesCount = stats.ActiveOpportunities;
            TotalApplicantsCount = stats.TotalApplications;
            TotalViews = stats.TotalViews;
            PendingReviewsCount = stats.PendingApplications;
            ShortlistedCandidatesCount = stats.ShortlistedApplications;
            ApplicationsReceivedCount = stats.TotalApplications;
            CandidatesShortlistedMetric = stats.ShortlistedApplications;
            RolesClosingSoonCount = 0;
            RecentOpportunities = stats.RecentOpportunities.Take(5).ToList();
            RecentApplicants = stats.RecentApplications.Take(5).ToList();
        }
    }
}
