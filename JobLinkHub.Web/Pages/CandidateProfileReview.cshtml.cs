using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace JobLinkHub.Web.Pages
{
    public class CandidateProfileReviewModel : PageModel
    {
        public int? Id { get; set; }

        public string CandidateInitials { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateEmail { get; set; } = string.Empty;
        public string ApplicationStatus { get; set; } = string.Empty;

        public string OpportunityTitle { get; set; } = string.Empty;
        public string OpportunityLocation { get; set; } = string.Empty;
        public string OpportunityType { get; set; } = string.Empty;
        public string ApplicationDate { get; set; } = string.Empty;

        public string ProfileSummary { get; set; } = string.Empty;
        public string Education { get; set; } = string.Empty;
        public string ExperienceLevel { get; set; } = string.Empty;
        public string CoverLetter { get; set; } = string.Empty;

        public List<string> Skills { get; set; } = new();
        public List<EvidenceItem> SkillEvidence { get; set; } = new();

        public void OnGet(int? id)
        {
            Id = id;

            // Temporary mock data for UI development.
            // Backend can later replace this using the passed application ID.
            CandidateInitials = "EA";
            CandidateName = "Emma A.";
            CandidateEmail = "emma@example.com";
            ApplicationStatus = "Pending Review";

            OpportunityTitle = "Frontend Developer Intern";
            OpportunityLocation = "Kigali";
            OpportunityType = "Internship";
            ApplicationDate = "18 Apr 2026, 10:24 AM";

            ProfileSummary = "Motivated early-career frontend developer with a strong interest in building responsive web interfaces and creating user-friendly digital experiences. Has completed personal and academic projects demonstrating practical ability in modern web development.";

            Education = "Bachelor’s Degree in Software Engineering";
            ExperienceLevel = "Entry Level / Internship";
            CoverLetter = "I am excited to apply for the Frontend Developer Intern opportunity because it aligns with my passion for creating intuitive and accessible digital products. Through academic projects and hands-on practice, I have developed skills in frontend development and collaborative problem-solving. I am eager to contribute, learn from your team, and continue growing in a professional environment.";

            Skills = new List<string>
            {
                "HTML",
                "CSS",
                "JavaScript",
                "Responsive Design",
                "Git",
                "Frontend Development"
            };

            SkillEvidence = new List<EvidenceItem>
            {
                new EvidenceItem
                {
                    Title = "Portfolio Project",
                    Description = "Responsive personal portfolio showcasing design and frontend projects."
                },
                new EvidenceItem
                {
                    Title = "Certificate",
                    Description = "Frontend Web Development certificate from an online learning platform."
                },
                new EvidenceItem
                {
                    Title = "GitHub Repository",
                    Description = "Source code for personal and academic frontend projects."
                }
            };
        }

        public class EvidenceItem
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }
    }
}