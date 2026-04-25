using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IOpportunityService _opportunityService;

    public AdminController(IAdminService adminService, IOpportunityService opportunityService)
    {
        _adminService = adminService;
        _opportunityService = opportunityService;
    }

    // GET api/admin/users
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] string? role,
        [FromQuery] string? keyword)
        => Ok(await _adminService.GetAllUsersAsync(role, keyword));

    // GET api/admin/users/{id}
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(long id)
    {
        var user = await _adminService.GetUserByIdAsync(id);
        if (user == null) return NotFound(new { message = "User not found" });
        return Ok(user);
    }

    // PUT api/admin/users/{id}/status
    [HttpPut("users/{id}/status")]
    public async Task<IActionResult> SetUserStatus(long id, [FromBody] BanUserDto dto)
    {
        var success = await _adminService.SetUserActiveStatusAsync(id, dto.IsActive);
        if (!success) return NotFound(new { message = "User not found" });
        return Ok(new { message = dto.IsActive ? "User activated" : "User deactivated" });
    }

    // DELETE api/admin/users/{id}
    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        var success = await _adminService.DeleteUserAsync(id);
        if (!success) return NotFound(new { message = "User not found" });
        return Ok(new { message = "User deleted" });
    }

    // GET api/admin/opportunities
    [HttpGet("opportunities")]
    public async Task<IActionResult> GetOpportunitiesForModeration()
        => Ok(await _adminService.GetAllOpportunitiesForModerationAsync());

    // DELETE api/admin/opportunities/{id}
    [HttpDelete("opportunities/{id}")]
    public async Task<IActionResult> DeleteOpportunity(long id)
    {
        var deleted = await _adminService.DeleteOpportunityAsync(id);
        if (!deleted) return NotFound(new { message = "Opportunity not found" });
        return Ok(new { message = "Opportunity removed" });
    }
}
