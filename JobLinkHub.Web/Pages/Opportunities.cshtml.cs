using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace JobLinkHub.Web.Pages
{
    public class OpportunitiesModel : PageModel
    {
        public List<OpportunityCard> Opportunities { get; set; } = new();

        public void OnGet()
        {
            // Temporary mock data for frontend development.
            // Backend can later replace this with real opportunity data.
            Opportunities = new List<OpportunityCard>
            {
                new OpportunityCard
                {
                    Title = "Frontend Developer Intern",
                    CompanyName = "Acme Studio",
                    ShortDescription = "Support the development of responsive and user-friendly web interfaces as part of a growing digital product team.",
                    Type = "Internship",
                    Status = "Open",
                    Location = "Kigali",
                    Category = "Technology",
                    Deadline = "Deadline: 20 Apr 2026",
                    Skills = new List<string> { "HTML", "CSS", "JavaScript" }
                },
                new OpportunityCard
                {
                    Title = "UI/UX Design Trainee",
                    CompanyName = "Pixel Forge",
                    ShortDescription = "Learn and contribute to product design workflows, wireframes, and user-centered digital experiences.",
                    Type = "Training",
                    Status = "Open",
                    Location = "Remote",
                    Category = "Design",
                    Deadline = "Deadline: 25 Apr 2026",
                    Skills = new List<string> { "Figma", "User Research", "Prototyping" }
                },
                new OpportunityCard
                {
                    Title = "Backend Developer",
                    CompanyName = "CodeBridge Labs",
                    ShortDescription = "Join the backend team to support API development, database integration, and scalable application logic.",
                    Type = "Job",
                    Status = "Open",
                    Location = "Kigali",
                    Category = "Technology",
                    Deadline = "Deadline: 15 Apr 2026",
                    Skills = new List<string> { "C#", ".NET", "SQL" }
                },
                new OpportunityCard
                {
                    Title = "Data Analyst Intern",
                    CompanyName = "Insight Core",
                    ShortDescription = "Work on data cleaning, dashboards, and reporting to support evidence-based decision-making.",
                    Type = "Internship",
                    Status = "Open",
                    Location = "Hybrid",
                    Category = "Data",
                    Deadline = "Deadline: 30 Apr 2026",
                    Skills = new List<string> { "Excel", "Power BI", "Data Analysis" }
                }
            };
        }

        public class OpportunityCard
        {
            public string Title { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public string ShortDescription { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Deadline { get; set; } = string.Empty;
            public List<string> Skills { get; set; } = new();
        }
    }
}