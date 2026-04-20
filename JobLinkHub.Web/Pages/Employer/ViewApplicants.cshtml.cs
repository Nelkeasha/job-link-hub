using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace JobLinkHub.Web.Pages
{
    public class ViewApplicantsModel : PageModel
    {
        public int TotalApplicationsCount { get; set; }
        public int PendingReviewCount { get; set; }
        public int ShortlistedCount { get; set; }
        public int RejectedCount { get; set; }

        public List<ApplicationRow> Applications { get; set; } = new();

        public void OnGet()
        {
            // Temporary mock data for frontend development.
            // Backend can later replace this with real service/database data.
            TotalApplicationsCount = 124;
            PendingReviewCount = 11;
            ShortlistedCount = 19;
            RejectedCount = 7;

            Applications = new List<ApplicationRow>
            {
                new ApplicationRow
                {
                    ApplicationId = 1,
                    FullName = "Emma A.",
                    Email = "emma@example.com",
                    OpportunityTitle = "Frontend Developer Intern",
                    SubmittedAt = "18 Apr 2026, 10:24 AM",
                    Status = "Pending Review",
                    SkillHighlight = "HTML, CSS, JavaScript"
                },
                new ApplicationRow
                {
                    ApplicationId = 2,
                    FullName = "Mitchelle K.",
                    Email = "mitchelle@example.com",
                    OpportunityTitle = "UI/UX Design Trainee",
                    SubmittedAt = "18 Apr 2026, 08:10 AM",
                    Status = "Shortlisted",
                    SkillHighlight = "Figma, User Research"
                },
                new ApplicationRow
                {
                    ApplicationId = 3,
                    FullName = "James O.",
                    Email = "james@example.com",
                    OpportunityTitle = "Backend Developer",
                    SubmittedAt = "17 Apr 2026, 04:42 PM",
                    Status = "Pending Review",
                    SkillHighlight = "C#, .NET, SQL"
                },
                new ApplicationRow
                {
                    ApplicationId = 4,
                    FullName = "Linda T.",
                    Email = "linda@example.com",
                    OpportunityTitle = "Data Analyst Intern",
                    SubmittedAt = "17 Apr 2026, 11:18 AM",
                    Status = "Rejected",
                    SkillHighlight = "Excel, Power BI"
                }
            };
        }

        public class ApplicationRow
        {
            public int ApplicationId { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string OpportunityTitle { get; set; } = string.Empty;
            public string SubmittedAt { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string SkillHighlight { get; set; } = string.Empty;
        }
    }
}