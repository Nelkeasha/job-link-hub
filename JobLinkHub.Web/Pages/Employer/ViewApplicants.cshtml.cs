using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.Employer
{
    [Authorize(Roles = "EMPLOYER")]
    public class ViewApplicantsModel : PageModel
    {
        private readonly IApplicationService _appService;
        private readonly IUserProfileService _profiles;
        private readonly IOpportunityService _opportunities;

        public ViewApplicantsModel(
            IApplicationService appService,
            IUserProfileService profiles,
            IOpportunityService opportunities)
        {
            _appService = appService;
            _profiles = profiles;
            _opportunities = opportunities;
        }

        [BindProperty(SupportsGet = true)] public long? OpportunityId { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }

        public int TotalApplicationsCount { get; set; }
        public int PendingReviewCount { get; set; }
        public int ShortlistedCount { get; set; }
        public int RejectedCount { get; set; }
        public List<ApplicationDto> Applications { get; set; } = new();
        public List<OpportunityDto> EmployerOpportunities { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return;

            var profile = await _profiles.GetEmployerByUserIdAsync(userId);
            if (profile == null) return;

            // Load employer's opportunities for the filter dropdown
            EmployerOpportunities = (await _opportunities.GetByEmployerAsync(profile.Id)).ToList();

            // Load applications: by specific opportunity or all for this employer
            List<ApplicationDto> all;
            if (OpportunityId.HasValue)
                all = (await _appService.GetByOpportunityAsync(OpportunityId.Value)).ToList();
            else
                all = (await _appService.GetByEmployerAsync(profile.Id)).ToList();

            TotalApplicationsCount = all.Count;
            PendingReviewCount  = all.Count(a => a.Status.Equals("PENDING",      StringComparison.OrdinalIgnoreCase));
            ShortlistedCount    = all.Count(a => a.Status.Equals("SHORTLISTED",  StringComparison.OrdinalIgnoreCase));
            RejectedCount       = all.Count(a => a.Status.Equals("REJECTED",     StringComparison.OrdinalIgnoreCase));

            Applications = (string.IsNullOrWhiteSpace(FilterStatus)
                ? all
                : all.Where(a => a.Status.Equals(FilterStatus, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync(long appId, string status)
        {
            await _appService.UpdateStatusAsync(appId, new UpdateApplicationStatusDto { Status = status });
            return RedirectToPage(new { OpportunityId, FilterStatus });
        }
    }
}
