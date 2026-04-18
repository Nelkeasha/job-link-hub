using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace JobLinkHub.Web.Pages
{
    public class ManageOpportunitiesModel : PageModel
    {
        public int TotalOpportunitiesCount { get; set; }
        public int LiveOpportunitiesCount { get; set; }
        public int DraftOpportunitiesCount { get; set; }
        public int ClosedOpportunitiesCount { get; set; }

        public List<OpportunityRow> Opportunities { get; set; } = new();

        public void OnGet()
        {
            // Temporary mock data for UI development.
            // Backend developers can later replace this with real service/database data.
            TotalOpportunitiesCount = 12;
            LiveOpportunitiesCount = 8;
            DraftOpportunitiesCount = 2;
            ClosedOpportunitiesCount = 2;

            Opportunities = new List<OpportunityRow>
            {
                new OpportunityRow
                {
                    Title = "Frontend Developer Intern",
                    Category = "Technology",
                    Type = "Internship",
                    Location = "Kigali",
                    Deadline = "20 Apr 2026",
                    ApplicantCount = 24,
                    Status = "Live"
                },
                new OpportunityRow
                {
                    Title = "UI/UX Design Trainee",
                    Category = "Design",
                    Type = "Training",
                    Location = "Remote",
                    Deadline = "25 Apr 2026",
                    ApplicantCount = 18,
                    Status = "Live"
                },
                new OpportunityRow
                {
                    Title = "Backend Developer",
                    Category = "Technology",
                    Type = "Full-time",
                    Location = "Kigali",
                    Deadline = "15 Apr 2026",
                    ApplicantCount = 31,
                    Status = "Review"
                },
                new OpportunityRow
                {
                    Title = "Product Marketing Associate",
                    Category = "Marketing",
                    Type = "Full-time",
                    Location = "Hybrid",
                    Deadline = "30 Apr 2026",
                    ApplicantCount = 12,
                    Status = "Draft"
                },
                new OpportunityRow
                {
                    Title = "Data Analyst Intern",
                    Category = "Data",
                    Type = "Internship",
                    Location = "Remote",
                    Deadline = "10 Apr 2026",
                    ApplicantCount = 27,
                    Status = "Closed"
                }
            };
        }

        public class OpportunityRow
        {
            public string Title { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string Deadline { get; set; } = string.Empty;
            public int ApplicantCount { get; set; }
            public string Status { get; set; } = string.Empty;
        }
    }
}