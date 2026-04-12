namespace JobLinkHub.Data.Entities;

public class JobSeekerSkill
{
    public long Id { get; set; }
    public long JobSeekerProfileId { get; set; }
    public long SkillId { get; set; }
    public string? ProficiencyLevel { get; set; }
    public int? YearsOfExperience { get; set; }

    public JobSeekerProfile JobSeekerProfile { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
    public ICollection<SkillEvidence> Evidence { get; set; } = new List<SkillEvidence>();
}