namespace JobLinkHub.Data.Entities;

public class Skill
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }

    public ICollection<JobSeekerSkill> JobSeekerSkills { get; set; } = new List<JobSeekerSkill>();
    public ICollection<OpportunitySkill> OpportunitySkills { get; set; } = new List<OpportunitySkill>();
}