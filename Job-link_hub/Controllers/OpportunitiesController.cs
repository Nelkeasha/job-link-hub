using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpportunitiesController : ControllerBase
{
    private readonly IOpportunityService _service;

    public OpportunitiesController(IOpportunityService service)
    {
        _service = service;
    }

    // GET api/opportunities
    // GET api/opportunities?keyword=developer&type=Job&location=Kigali&status=Active
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? keyword,
        [FromQuery] string? type,
        [FromQuery] string? location,
        [FromQuery] string? status)
    {
        var opportunities = await _service.GetAllAsync(keyword, type, location, status);
        return Ok(opportunities);
    }

    // GET api/opportunities/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var opportunity = await _service.GetByIdAsync(id);
        if (opportunity == null)
            return NotFound(new { message = "Opportunity not found" });

        // Track view
        await _service.IncrementViewsAsync(id);
        return Ok(opportunity);
    }

    // GET api/opportunities/employer/3
    [HttpGet("employer/{employerProfileId}")]
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    public async Task<IActionResult> GetByEmployer(long employerProfileId)
    {
        var opportunities = await _service.GetByEmployerAsync(employerProfileId);
        return Ok(opportunities);
    }

    // POST api/opportunities
    [HttpPost]
    [Authorize(Roles = "EMPLOYER")]
    public async Task<IActionResult> Create(
        [FromBody] CreateOpportunityDto dto,
        [FromQuery] long employerProfileId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _service.CreateAsync(employerProfileId, dto);
        return CreatedAtAction(nameof(GetById),
            new { id = created.Id }, created);
    }

    // PUT api/opportunities/5
    [HttpPut("{id}")]
    [Authorize(Roles = "EMPLOYER")]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateOpportunityDto dto,
        [FromQuery] long employerProfileId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _service.UpdateAsync(id, employerProfileId, dto);
        if (updated == null)
            return NotFound(new { message = "Opportunity not found or you don't own it" });

        return Ok(updated);
    }

    // DELETE api/opportunities/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    public async Task<IActionResult> Delete(
        long id,
        [FromQuery] long employerProfileId)
    {
        var deleted = await _service.DeleteAsync(id, employerProfileId);
        if (!deleted)
            return NotFound(new { message = "Opportunity not found or you don't own it" });

        return Ok(new { message = "Opportunity deleted successfully" });
    }

    // GET api/opportunities/count
    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        var count = await _service.GetTotalCountAsync();
        return Ok(new { total = count });
    }
}