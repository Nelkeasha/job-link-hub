using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "CANDIDATE")]
public class SavedJobsController : ControllerBase
{
    private readonly ISavedJobService _service;

    public SavedJobsController(ISavedJobService service)
    {
        _service = service;
    }

    // GET api/savedjobs/jobseeker/1
    [HttpGet("jobseeker/{jobSeekerProfileId}")]
    public async Task<IActionResult> GetByJobSeeker(long jobSeekerProfileId)
    {
        var savedJobs = await _service.GetByJobSeekerAsync(jobSeekerProfileId);
        return Ok(savedJobs);
    }

    // POST api/savedjobs
    [HttpPost]
    public async Task<IActionResult> Save(
        [FromBody] SaveJobDto dto,
        [FromQuery] long jobSeekerProfileId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check if already saved
        var alreadySaved = await _service.IsSavedAsync(
            jobSeekerProfileId, dto.OpportunityId);

        if (alreadySaved)
            return Conflict(new { message = "Opportunity already saved" });

        var saved = await _service.SaveJobAsync(jobSeekerProfileId, dto);
        return Ok(saved);
    }

    // DELETE api/savedjobs/5?jobSeekerProfileId=1
    [HttpDelete("{opportunityId}")]
    public async Task<IActionResult> Unsave(
        long opportunityId,
        [FromQuery] long jobSeekerProfileId)
    {
        var removed = await _service.UnsaveJobAsync(jobSeekerProfileId, opportunityId);
        if (!removed)
            return NotFound(new { message = "Saved job not found" });

        return Ok(new { message = "Opportunity removed from saved list" });
    }

    // GET api/savedjobs/check?jobSeekerProfileId=1&opportunityId=2
    [HttpGet("check")]
    public async Task<IActionResult> IsSaved(
        [FromQuery] long jobSeekerProfileId,
        [FromQuery] long opportunityId)
    {
        var isSaved = await _service.IsSavedAsync(jobSeekerProfileId, opportunityId);
        return Ok(new { isSaved });
    }

    // GET api/savedjobs/count/1
    [HttpGet("count/{jobSeekerProfileId}")]
    public async Task<IActionResult> GetCount(long jobSeekerProfileId)
    {
        var count = await _service.GetCountAsync(jobSeekerProfileId);
        return Ok(new { count });
    }
}