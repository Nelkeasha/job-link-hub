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
                    FullDescription = "This internship opportunity is designed for emerging frontend developers who want to strengthen their practical experience in building modern, responsive, and user-friendly interfaces. The selected candidate will work closely with the product and engineering team, contribute to live projects, and gain exposure to collaborative development workflows.",
                    QualificationRequired = "Open to students, recent graduates, and early-career professionals with foundational frontend skills.",
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
                    FullDescription = "This training opportunity is ideal for aspiring UI/UX designers who want to build confidence in product design, wireframing, prototyping, and user-centered problem solving. The trainee will support design tasks, participate in reviews, and gain exposure to real digital product workflows.",
                    QualificationRequired = "Suitable for learners and graduates with interest or background in design, digital products, or user experience.",
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
                    FullDescription = "This role is intended for candidates who are interested in backend systems, APIs, and data-driven application development. The selected candidate will contribute to backend logic, database interaction, and structured service development while collaborating with other members of the engineering team.",
                    QualificationRequired = "Degree, diploma, or practical experience in software development, backend engineering, or a related field.",
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
                    FullDescription = "This internship is meant for candidates interested in working with data, reporting, and analytics. The intern will support data preparation, dashboard creation, and basic insights generation to help teams make informed decisions.",
                    QualificationRequired = "Open to students and graduates with interest in data analysis, reporting, or business intelligence.",
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
            public string FullDescription { get; set; } = string.Empty;
            public string QualificationRequired { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Deadline { get; set; } = string.Empty;
            public List<string> Skills { get; set; } = new();
        }
    }
}