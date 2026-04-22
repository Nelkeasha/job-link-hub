using JobLinkHub.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Authorize(Roles = "CANDIDATE")]
[Route("api/recommendations")]
public class RecommendationsController(
    IRecommendationService recommendationService,
    IUserContextService userContextService) : ControllerBase
{
    [HttpGet("opportunities")]
    public async Task<IActionResult> GetRecommendedOpportunities([FromQuery] int take = 10)
    {
        var user = await userContextService.GetCurrentUserAsync(User);
        if (user is null) return Unauthorized();

        var result = await recommendationService.GetRecommendedOpportunitiesAsync(user.Id, take);
        return Ok(result);
    }
}
