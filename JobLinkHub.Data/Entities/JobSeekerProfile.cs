using static System.Net.Mime.MediaTypeNames;

namespace JobLinkHub.Data.Entities;

public class JobSeekerProfile
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string? Bio { get; set; }
    public string? CareerInterest { get; set; }
    public string? EducationLevel { get; set; }
    public string? Institution { get; set; }
    public int? GraduationYear { get; set; }
    public string? ResumeUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<JobSeekerSkill> Skills { get; set; } = new List<JobSeekerSkill>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
}