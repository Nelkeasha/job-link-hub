using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST api/auth/register/candidate
    [HttpPost("register/candidate")]
    public async Task<IActionResult> RegisterCandidate(
        [FromBody] RegisterCandidateDto dto)
    {
        try
        {
            var response = await _authService.RegisterCandidateAsync(dto);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST api/auth/register/employer
    [HttpPost("register/employer")]
    public async Task<IActionResult> RegisterEmployer(
        [FromBody] RegisterEmployerDto dto)
    {
        try
        {
            var response = await _authService.RegisterEmployerAsync(dto);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var response = await _authService.LoginAsync(dto);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // PUT api/auth/change-password
    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            var userId = long.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _authService.ChangePasswordAsync(userId, dto);

            return result
                ? Ok(new { message = "Password changed successfully" })
                : BadRequest(new { message = "Failed to change password" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET api/auth/me
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.FindFirstValue(ClaimTypes.Email),
            FirstName = User.FindFirstValue(ClaimTypes.GivenName),
            LastName = User.FindFirstValue(ClaimTypes.Surname),
            Role = User.FindFirstValue(ClaimTypes.Role),
            ProfileId = User.FindFirstValue("ProfileId")
        });
    }
}