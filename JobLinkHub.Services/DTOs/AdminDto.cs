namespace JobLinkHub.Services.DTOs;

public class UserListDto
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ProfileId { get; set; }
}

public class BanUserDto
{
    public bool IsActive { get; set; }
}

public class AdminOpportunityDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string OpportunityType { get; set; } = string.Empty;
    public int Views { get; set; }
    public int ApplicationCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
