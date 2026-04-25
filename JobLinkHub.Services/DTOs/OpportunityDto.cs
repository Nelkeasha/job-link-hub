namespace JobLinkHub.Services.DTOs;

public class OpportunityDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OpportunityType { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? QualificationRequired { get; set; }
    public string? SalaryRange { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Views { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyDescription { get; set; }
    public string? CompanyLogo { get; set; }
    public long EmployerProfileId { get; set; }
    public int ApplicationCount { get; set; }
    public List<string> RequiredSkills { get; set; } = new();
}

public class CreateOpportunityDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OpportunityType { get; set; } = "Job";
    public string? Location { get; set; }
    public string? QualificationRequired { get; set; }
    public string? SalaryRange { get; set; }
    public DateTime? Deadline { get; set; }
    public string Status { get; set; } = "Active";
    public List<long> SkillIds { get; set; } = new();
}

public class UpdateOpportunityDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OpportunityType { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? QualificationRequired { get; set; }
    public string? SalaryRange { get; set; }
    public DateTime? Deadline { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<long> SkillIds { get; set; } = new();
}