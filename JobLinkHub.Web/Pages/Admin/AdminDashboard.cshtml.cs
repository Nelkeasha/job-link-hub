using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Web.Pages.Admin
{
    [Authorize(Roles = "ADMIN")]
    public class AdminDashboardModel : PageModel
    {
        private readonly IDashboardService _dashboard;
        public AdminDashboardModel(IDashboardService dashboard) { _dashboard = dashboard; }

        public int TotalUsers { get; set; }
        public int TotalJobSeekers { get; set; }
        public int TotalEmployers { get; set; }
        public int TotalOpportunities { get; set; }
        public int TotalApplications { get; set; }
        public int NewUsersThisMonth { get; set; }

        public async Task OnGetAsync()
        {
            var stats = await _dashboard.GetAdminDashboardAsync();
            TotalUsers = stats.TotalUsers;
            TotalJobSeekers = stats.TotalCandidates;
            TotalEmployers = stats.TotalEmployers;
            TotalOpportunities = stats.ActiveOpportunities;
            TotalApplications = stats.TotalApplications;
            NewUsersThisMonth = stats.NewUsersThisMonth;
        }
    }
}