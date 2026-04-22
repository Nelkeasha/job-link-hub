namespace JobLinkHub.API.Models.Recommendations;

public class RecommendedOpportunityDto
{
    public long OpportunityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OpportunityType { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? EmployerName { get; set; }
    public int MatchingSkillsCount { get; set; }
}
