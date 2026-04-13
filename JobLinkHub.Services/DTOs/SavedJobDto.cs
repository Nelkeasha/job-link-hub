namespace JobLinkHub.Services.DTOs;

public class SavedJobDto
{
    public long Id { get; set; }
    public long OpportunityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string OpportunityType { get; set; } = string.Empty;
    public string? SalaryRange { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime SavedAt { get; set; }
    public string? Notes { get; set; }
}

public class SaveJobDto
{
    public long OpportunityId { get; set; }
    public string? Notes { get; set; }
}