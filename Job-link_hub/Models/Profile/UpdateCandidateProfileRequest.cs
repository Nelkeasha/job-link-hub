namespace JobLinkHub.API.Models.Profile;

public class UpdateCandidateProfileRequest
{
    public string? Bio { get; set; }
    public string? CareerInterest { get; set; }
    public string? EducationLevel { get; set; }
    public string? Institution { get; set; }
    public int? GraduationYear { get; set; }
    public string? ResumeUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Location { get; set; }
}
