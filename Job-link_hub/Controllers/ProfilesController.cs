using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly IUserProfileService _service;
    public ProfilesController(IUserProfileService service) { _service = service; }

    // GET api/profiles/candidates
    [HttpGet("candidates")]
    public async Task<IActionResult> GetAllCandidates(
        [FromQuery] string? keyword,
        [FromQuery] string? location)
    {
        var profiles = await _service.GetAllCandidatesAsync(keyword, location);
        return Ok(profiles);
    }

    // GET api/profiles/candidates/{id}
    [HttpGet("candidates/{id}")]
    public async Task<IActionResult> GetCandidateById(long id)
    {
        var profile = await _service.GetCandidateByIdAsync(id);
        if (profile == null) return NotFound(new { message = "Candidate profile not found" });
        return Ok(profile);
    }

    // GET api/profiles/candidates/user/{userId}
    [HttpGet("candidates/user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetCandidateByUserId(long userId)
    {
        var profile = await _service.GetCandidateByUserIdAsync(userId);
        if (profile == null) return NotFound(new { message = "Candidate profile not found" });
        return Ok(profile);
    }

    // PUT api/profiles/candidates/{userId}
    [HttpPut("candidates/{userId}")]
    [Authorize(Roles = "CANDIDATE,ADMIN")]
    public async Task<IActionResult> UpdateCandidate(long userId, [FromBody] UpdateCandidateProfileDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var updated = await _service.UpdateCandidateAsync(userId, dto);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET api/profiles/employers
    [HttpGet("employers")]
    public async Task<IActionResult> GetAllEmployers(
        [FromQuery] string? keyword,
        [FromQuery] string? location)
    {
        var profiles = await _service.GetAllEmployersAsync(keyword, location);
        return Ok(profiles);
    }

    // GET api/profiles/employers/{id}
    [HttpGet("employers/{id}")]
    public async Task<IActionResult> GetEmployerById(long id)
    {
        var profile = await _service.GetEmployerByIdAsync(id);
        if (profile == null) return NotFound(new { message = "Employer profile not found" });
        return Ok(profile);
    }

    // GET api/profiles/employers/user/{userId}
    [HttpGet("employers/user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetEmployerByUserId(long userId)
    {
        var profile = await _service.GetEmployerByUserIdAsync(userId);
        if (profile == null) return NotFound(new { message = "Employer profile not found" });
        return Ok(profile);
    }

    // PUT api/profiles/employers/{userId}
    [HttpPut("employers/{userId}")]
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    public async Task<IActionResult> UpdateEmployer(long userId, [FromBody] UpdateEmployerProfileDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var updated = await _service.UpdateEmployerAsync(userId, dto);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
