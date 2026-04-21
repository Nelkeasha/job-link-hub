using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet("opportunities")]
    [Authorize(Roles = "CANDIDATE")]
    public async Task<IActionResult> GetMatchedOpportunities([FromQuery] int top = 10)
    {
        var profileId = User.FindFirstValue("ProfileId");
        if (string.IsNullOrEmpty(profileId))
            return BadRequest(new { message = "Profile not found" });

        var results = await _recommendationService
            .GetRecommendationsForCandidateAsync(long.Parse(profileId), top);
        return Ok(results);
    }

    [HttpGet("candidates/{opportunityId}")]
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    public async Task<IActionResult> GetMatchedCandidates(long opportunityId, [FromQuery] int top = 10)
    {
        var results = await _recommendationService
            .GetMatchingCandidatesForOpportunityAsync(opportunityId, top);
        return Ok(results);
    }
}
