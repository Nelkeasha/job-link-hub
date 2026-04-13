namespace JobLinkHub.Services.DTOs;

public class ApplicationDto
{
    public long Id { get; set; }
    public long OpportunityId { get; set; }
    public string OpportunityTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public long JobSeekerProfileId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CoverLetter { get; set; }
    public string? ResumeUsed { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime ApplicationDate { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateApplicationDto
{
    public long OpportunityId { get; set; }
    public string? CoverLetter { get; set; }
    public string? ResumeUsed { get; set; }
}

public class UpdateApplicationStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
}