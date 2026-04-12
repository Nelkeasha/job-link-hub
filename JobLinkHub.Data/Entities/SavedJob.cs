namespace JobLinkHub.Data.Entities;

public class SavedJob
{
    public long Id { get; set; }
    public long JobSeekerProfileId { get; set; }
    public long OpportunityId { get; set; }
    public string? Notes { get; set; }
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    public JobSeekerProfile JobSeekerProfile { get; set; } = null!;
    public Opportunity Opportunity { get; set; } = null!;
}