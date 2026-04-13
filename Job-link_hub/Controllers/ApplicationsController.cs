using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _service;

    public ApplicationsController(IApplicationService service)
    {
        _service = service;
    }

    // GET api/applications/opportunity/5
    [HttpGet("opportunity/{opportunityId}")]
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    public async Task<IActionResult> GetByOpportunity(long opportunityId)
    {
        var applications = await _service.GetByOpportunityAsync(opportunityId);
        return Ok(applications);
    }

    // GET api/applications/jobseeker/3
    [HttpGet("jobseeker/{jobSeekerProfileId}")]
    [Authorize(Roles = "CANDIDATE,ADMIN")]
    public async Task<IActionResult> GetByJobSeeker(long jobSeekerProfileId)
    {
        var applications = await _service.GetByJobSeekerAsync(jobSeekerProfileId);
        return Ok(applications);
    }

    // GET api/applications/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var application = await _service.GetByIdAsync(id);
        if (application == null)
            return NotFound(new { message = "Application not found" });

        return Ok(application);
    }

    // POST api/applications
    [HttpPost]
    [Authorize(Roles = "CANDIDATE")]
    public async Task<IActionResult> Create(
        [FromBody] CreateApplicationDto dto,
        [FromQuery] long jobSeekerProfileId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check if already applied
        var alreadyApplied = await _service.HasAppliedAsync(
            jobSeekerProfileId, dto.OpportunityId);

        if (alreadyApplied)
            return Conflict(new { message = "You have already applied for this opportunity" });

        var created = await _service.CreateAsync(jobSeekerProfileId, dto);
        return CreatedAtAction(nameof(GetById),
            new { id = created.Id }, created);
    }

    // PUT api/applications/5/status
    [HttpPut("{id}/status")]
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    public async Task<IActionResult> UpdateStatus(
        long id,
        [FromBody] UpdateApplicationStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _service.UpdateStatusAsync(id, dto);
        if (updated == null)
            return NotFound(new { message = "Application not found" });

        return Ok(updated);
    }

    // PUT api/applications/5/withdraw
    [HttpPut("{id}/withdraw")]
    [Authorize(Roles = "CANDIDATE")]
    public async Task<IActionResult> Withdraw(
        long id,
        [FromQuery] long jobSeekerProfileId)
    {
        var withdrawn = await _service.WithdrawAsync(id, jobSeekerProfileId);
        if (!withdrawn)
            return NotFound(new { message = "Application not found or you don't own it" });

        return Ok(new { message = "Application withdrawn successfully" });
    }

    // GET api/applications/check?jobSeekerProfileId=1&opportunityId=2
    [HttpGet("check")]
    [Authorize(Roles = "CANDIDATE")]
    public async Task<IActionResult> CheckApplied(
        [FromQuery] long jobSeekerProfileId,
        [FromQuery] long opportunityId)
    {
        var hasApplied = await _service.HasAppliedAsync(jobSeekerProfileId, opportunityId);
        return Ok(new { hasApplied });
    }
}