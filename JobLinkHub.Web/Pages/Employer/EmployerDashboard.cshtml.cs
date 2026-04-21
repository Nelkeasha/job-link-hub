using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace JobLinkHub.Web.Pages
{
    public class EmployerDashboardModel : PageModel
    {
        public string CompanyName { get; set; } = string.Empty;

        public int ActiveOpportunitiesCount { get; set; }
        public int TotalApplicantsCount { get; set; }
        public int ShortlistedCandidatesCount { get; set; }
        public int PendingReviewsCount { get; set; }

        public int ApplicationsReceivedCount { get; set; }
        public int CandidatesShortlistedMetric { get; set; }
        public int RolesClosingSoonCount { get; set; }

        public List<OpportunityItem> RecentOpportunities { get; set; } = new();
        public List<ApplicantItem> RecentApplicants { get; set; } = new();

        public void OnGet()
        {
            // Temporary mock data for UI development.
            // Backend developers can later replace this with real service/database data.
            CompanyName = "Acme Studio";

            ActiveOpportunitiesCount = 8;
            TotalApplicantsCount = 124;
            ShortlistedCandidatesCount = 19;
            PendingReviewsCount = 11;

            ApplicationsReceivedCount = 43;
            CandidatesShortlistedMetric = 9;
            RolesClosingSoonCount = 3;

            RecentOpportunities = new List<OpportunityItem>
            {
                new OpportunityItem
                {
                    Title = "Frontend Developer Intern",
                    Meta = "Kigali • Internship • 24 applicants",
                    Status = "Live"
                },
                new OpportunityItem
                {
                    Title = "UI/UX Design Trainee",
                    Meta = "Remote • Training • 18 applicants",
                    Status = "Live"
                },
                new OpportunityItem
                {
                    Title = "Backend Developer",
                    Meta = "Kigali • Full-time • 31 applicants",
                    Status = "Reviewing"
                }
            };

            RecentApplicants = new List<ApplicantItem>
            {
                new ApplicantItem
                {
                    Initials = "EA",
                    FullName = "Emma A.",
                    AppliedRole = "Applied for Frontend Developer Intern"
                },
                new ApplicantItem
                {
                    Initials = "MK",
                    FullName = "Mitchelle K.",
                    AppliedRole = "Applied for UI/UX Design Trainee"
                },
                new ApplicantItem
                {
                    Initials = "JO",
                    FullName = "James O.",
                    AppliedRole = "Applied for Backend Developer"
                }
            };
        }

        public class OpportunityItem
        {
            public string Title { get; set; } = string.Empty;
            public string Meta { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }

        public class ApplicantItem
        {
            public string Initials { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string AppliedRole { get; set; } = string.Empty;
        }
    }
}