namespace JobLinkHub.Services.DTOs;

public class MatchedOpportunityDto
{
    public OpportunityDto Opportunity { get; set; } = null!;
    public double MatchScore { get; set; }
    public int MatchedSkillCount { get; set; }
    public int TotalRequiredSkills { get; set; }
    public List<string> MatchedSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
}

public class MatchedCandidateDto
{
    public long JobSeekerProfileId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Location { get; set; }
    public double MatchScore { get; set; }
    public int MatchedSkillCount { get; set; }
    public int TotalRequiredSkills { get; set; }
    public List<string> MatchedSkills { get; set; } = new();
    public List<string> MissingSkills { get; set; } = new();
}
