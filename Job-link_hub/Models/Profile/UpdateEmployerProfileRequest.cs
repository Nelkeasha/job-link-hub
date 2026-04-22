namespace JobLinkHub.API.Models.Profile;

public class UpdateEmployerProfileRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyType { get; set; }
    public string? Industry { get; set; }
    public string? CompanyDescription { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
    public string? LogoUrl { get; set; }
}
