using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.Public
{
    public class OpportunitiesModel : PageModel
    {
        private readonly IOpportunityService _opportunities;

        public OpportunitiesModel(IOpportunityService opportunities)
        {
            _opportunities = opportunities;
        }

        [BindProperty(SupportsGet = true)] public string? SearchQ { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterType { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterLocation { get; set; }

        public List<OpportunityDto> Opportunities { get; set; } = new();

        public async Task OnGetAsync()
        {
            var type = string.IsNullOrWhiteSpace(FilterType) || FilterType == "All Types" ? null : FilterType;
            var location = string.IsNullOrWhiteSpace(FilterLocation) || FilterLocation == "All Locations" ? null : FilterLocation;

            Opportunities = (await _opportunities.GetAllAsync(SearchQ, type, location, "Active")).ToList();
        }
    }
}