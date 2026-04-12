using static System.Net.Mime.MediaTypeNames;

namespace JobLinkHub.Data.Entities;

public class Opportunity
{
    public long Id { get; set; }
    public long EmployerProfileId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OpportunityType { get; set; } = "Job";
    public string? Location { get; set; }
    public string? QualificationRequired { get; set; }
    public string? SalaryRange { get; set; }
    public string Status { get; set; } = "Active";
    public int Views { get; set; } = 0;
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public EmployerProfile EmployerProfile { get; set; } = null!;
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    public ICollection<OpportunitySkill> RequiredSkills { get; set; } = new List<OpportunitySkill>();
}