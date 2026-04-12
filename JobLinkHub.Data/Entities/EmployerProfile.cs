namespace JobLinkHub.Data.Entities;

public class EmployerProfile
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyType { get; set; }
    public string? Industry { get; set; }
    public string? CompanyDescription { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public string? LogoUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}