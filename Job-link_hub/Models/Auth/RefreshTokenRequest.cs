namespace JobLinkHub.API.Models.Auth;

public class RefreshTokenRequest
{
    public string Email { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
