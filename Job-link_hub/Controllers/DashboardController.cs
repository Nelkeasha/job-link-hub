using JobLinkHub.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController(
    IDashboardService dashboardService,
    IUserContextService userContextService) : ControllerBase
{
    [HttpGet("candidate/stats")]
    [Authorize(Roles = "CANDIDATE")]
    public async Task<IActionResult> GetCandidateStats()
    {
        var user = await userContextService.GetCurrentUserAsync(User);
        if (user is null) return Unauthorized();

        var result = await dashboardService.GetCandidateStatsAsync(user.Id);
        return Ok(result);
    }

    [HttpGet("employer/stats")]
    [Authorize(Roles = "EMPLOYER")]
    public async Task<IActionResult> GetEmployerStats()
    {
        var user = await userContextService.GetCurrentUserAsync(User);
        if (user is null) return Unauthorized();

        var result = await dashboardService.GetEmployerStatsAsync(user.Id);
        return Ok(result);
    }

    [HttpGet("admin/stats")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAdminStats()
    {
        var result = await dashboardService.GetAdminStatsAsync();
        return Ok(result);
    }
}
