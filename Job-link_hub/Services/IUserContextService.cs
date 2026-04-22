using JobLinkHub.Data.Entities;
using System.Security.Claims;

namespace JobLinkHub.API.Services;

public interface IUserContextService
{
    Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal);
}
