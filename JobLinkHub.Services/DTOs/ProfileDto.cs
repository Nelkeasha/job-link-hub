namespace JobLinkHub.Services.DTOs;

public class CandidateProfileDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public string? CareerInterest { get; set; }
    public string? EducationLevel { get; set; }
    public string? Institution { get; set; }
    public int? GraduationYear { get; set; }
    public string? ResumeUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Skills { get; set; } = new();
    public List<long> SkillIds { get; set; } = new();
}

public class UpdateCandidateProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public string? CareerInterest { get; set; }
    public string? EducationLevel { get; set; }
    public string? Institution { get; set; }
    public int? GraduationYear { get; set; }
    public string? ResumeUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Location { get; set; }
    public List<long> SkillIds { get; set; } = new();
}

public class EmployerProfileDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyType { get; set; }
    public string? Industry { get; set; }
    public string? CompanyDescription { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public string? LogoUrl { get; set; }
    public int ActiveOpportunitiesCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateEmployerProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyType { get; set; }
    public string? Industry { get; set; }
    public string? CompanyDescription { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public string? LogoUrl { get; set; }
}
