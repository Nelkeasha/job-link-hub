using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    public class DashboardModel : PageModel
    {
        // Lightweight properties expected by the Razor page. Backend will populate when ready.
        public string FullName { get; set; } = "";
        public int TotalApplications { get; set; }
        public int SavedJobsCount { get; set; }
        public List<RecentApp> RecentApplications { get; set; } = new();

        public class RecentApp
        {
            public string OpportunityTitle { get; set; } = "";
            public string EmployerName { get; set; } = "";
            public DateTime AppliedAt { get; set; }
            public string? Status { get; set; }
        }

        public void OnGet()
        {
            // keep defaults; backend developer will fill real data
        }
    }
}
