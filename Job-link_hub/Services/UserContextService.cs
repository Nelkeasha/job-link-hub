using JobLinkHub.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace JobLinkHub.API.Services;

public class UserContextService(UserManager<User> userManager) : IUserContextService
{
    public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirst("sub")?.Value
                     ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrWhiteSpace(userId) ? null : await userManager.FindByIdAsync(userId);
    }
}
