namespace JobLinkHub.Data.Entities;

public class Application
{
    public long Id { get; set; }
    public long OpportunityId { get; set; }
    public long JobSeekerProfileId { get; set; }
    public string Status { get; set; } = "PENDING";
    public string? CoverLetter { get; set; }
    public string? ResumeUsed { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Opportunity Opportunity { get; set; } = null!;
    public JobSeekerProfile JobSeekerProfile { get; set; } = null!;
}