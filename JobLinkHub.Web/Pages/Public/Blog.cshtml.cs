using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobLinkHub.Web.Pages.Public
{
    public class BlogModel : PageModel
    {
        public List<BlogPost> Posts { get; set; } = new();

        public void OnGet()
        {
            Posts = new List<BlogPost>
            {
                new BlogPost { Slug = "how-to-build-a-skill-first-profile", Title = "How to Build a Skill-First Profile That Gets Noticed", Summary = "Learn how to structure your JobLink Hub profile around verifiable skills to stand out to employers.", Category = "Career Tips", Date = new DateTime(2026, 3, 10), ReadMinutes = 5 },
                new BlogPost { Slug = "top-tech-skills-2026", Title = "Top 10 Tech Skills Employers Are Looking for in 2026", Summary = "From AI fundamentals to cloud DevOps — here are the skills that employers on JobLink Hub are searching for most.", Category = "Industry Insights", Date = new DateTime(2026, 3, 18), ReadMinutes = 7 },
                new BlogPost { Slug = "how-to-write-a-cover-letter", Title = "How to Write a Cover Letter That Actually Gets Read", Summary = "Most cover letters are ignored. Here's a concise framework that makes yours stand out — with examples.", Category = "Career Tips", Date = new DateTime(2026, 3, 28), ReadMinutes = 6 },
                new BlogPost { Slug = "employer-guide-posting-opportunities", Title = "Employer's Guide: How to Post Opportunities That Attract Top Talent", Summary = "Structure your job postings for clarity and specificity. A well-written post can double your qualified applications.", Category = "For Employers", Date = new DateTime(2026, 4, 5), ReadMinutes = 4 },
                new BlogPost { Slug = "navigating-internships-students", Title = "Navigating Internships as a Student in 2026", Summary = "Internships are competitive. Here's how students can use JobLink Hub's matching tools to land the right one.", Category = "Students", Date = new DateTime(2026, 4, 12), ReadMinutes = 5 },
            };
        }

        public class BlogPost
        {
            public string Slug { get; set; } = "";
            public string Title { get; set; } = "";
            public string Summary { get; set; } = "";
            public string Category { get; set; } = "";
            public DateTime Date { get; set; }
            public int ReadMinutes { get; set; }
        }
    }
}
