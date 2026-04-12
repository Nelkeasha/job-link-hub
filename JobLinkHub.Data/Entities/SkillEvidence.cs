namespace JobLinkHub.Data.Entities;

public class SkillEvidence
{
    public long Id { get; set; }
    public long JobSeekerSkillId { get; set; }
    public string EvidenceType { get; set; } = string.Empty;
    public string EvidenceLink { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public JobSeekerSkill JobSeekerSkill { get; set; } = null!;
}