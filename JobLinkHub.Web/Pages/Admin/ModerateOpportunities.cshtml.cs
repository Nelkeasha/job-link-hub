using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.Admin
{
    [Authorize(Roles = "ADMIN")]
    public class ModerateOpportunitiesModel : PageModel
    {
        private readonly IAdminService _admin;
        public ModerateOpportunitiesModel(IAdminService admin) { _admin = admin; }

        [BindProperty(SupportsGet = true)] public string? SearchQ { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterType { get; set; }

        public List<AdminOpportunityDto> Opportunities { get; set; } = new();
        public string? Message { get; set; }

        public async Task OnGetAsync()
        {
            var all = await _admin.GetAllOpportunitiesForModerationAsync();

            if (!string.IsNullOrWhiteSpace(SearchQ))
                all = all.Where(o =>
                    o.Title.Contains(SearchQ, StringComparison.OrdinalIgnoreCase) ||
                    o.CompanyName.Contains(SearchQ, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(FilterType))
                all = all.Where(o => o.OpportunityType == FilterType);

            Opportunities = all.ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            var ok = await _admin.DeleteOpportunityAsync(id);
            Message = ok ? "Opportunity removed." : "Not found.";
            return RedirectToPage(new { SearchQ, FilterType });
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(long id, string currentStatus)
        {
            var ok = await _admin.ToggleOpportunityStatusAsync(id, currentStatus);
            Message = ok ? "Opportunity status updated." : "Not found.";
            return RedirectToPage(new { SearchQ, FilterType });
        }
    }
}