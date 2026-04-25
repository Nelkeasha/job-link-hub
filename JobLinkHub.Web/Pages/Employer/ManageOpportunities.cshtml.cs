using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.Employer
{
    [Authorize(Roles = "EMPLOYER")]
    public class ManageOpportunitiesModel : PageModel
    {
        private readonly IOpportunityService _opportunities;
        private readonly IUserProfileService _profiles;

        public ManageOpportunitiesModel(IOpportunityService opportunities, IUserProfileService profiles)
        {
            _opportunities = opportunities;
            _profiles = profiles;
        }

        [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }

        public int TotalOpportunitiesCount { get; set; }
        public int LiveOpportunitiesCount { get; set; }
        public int DraftOpportunitiesCount { get; set; }
        public int ClosedOpportunitiesCount { get; set; }

        public List<OpportunityDto> Opportunities { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return;

            var profile = await _profiles.GetEmployerByUserIdAsync(userId);
            if (profile == null) return;

            var all = (await _opportunities.GetByEmployerAsync(profile.Id)).ToList();

            TotalOpportunitiesCount = all.Count;
            LiveOpportunitiesCount   = all.Count(o => o.Status == "Active");
            DraftOpportunitiesCount  = all.Count(o => o.Status == "Draft");
            ClosedOpportunitiesCount = all.Count(o => o.Status == "Closed");

            Opportunities = (string.IsNullOrWhiteSpace(FilterStatus)
                ? all
                : all.Where(o => o.Status.Equals(FilterStatus, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userIdStr, out var userId)) return RedirectToPage();

            var profile = await _profiles.GetEmployerByUserIdAsync(userId);
            if (profile != null)
            {
                await _opportunities.DeleteAsync(id, profile.Id);
                TempData["SuccessMessage"] = "Opportunity deleted.";
            }

            return RedirectToPage();
        }
    }
}