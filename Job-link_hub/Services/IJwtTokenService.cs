using JobLinkHub.Data.Entities;

namespace JobLinkHub.API.Services;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IList<string> roles);
    string GenerateRefreshToken();
}
