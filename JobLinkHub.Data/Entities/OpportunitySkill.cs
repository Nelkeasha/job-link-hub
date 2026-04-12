namespace JobLinkHub.Data.Entities;

public class OpportunitySkill
{
    public long OpportunityId { get; set; }
    public long SkillId { get; set; }

    public Opportunity Opportunity { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}