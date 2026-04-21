using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
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

    [HttpPost("register/candidate")]
    public async Task<IActionResult> RegisterCandidate([FromBody] RegisterCandidateDto dto)
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

    [HttpPost("register/employer")]
    public async Task<IActionResult> RegisterEmployer([FromBody] RegisterEmployerDto dto)
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

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var response = await _authService.LoginAsync(dto, ipAddress);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto dto)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var response = await _authService.RefreshTokenAsync(dto.RefreshToken, ipAddress);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] TokenRequestDto dto)
    {
        try
        {
            var ipAddress = GetIpAddress();
            await _authService.RevokeTokenAsync(dto.RefreshToken, ipAddress);
            return Ok(new { message = "Token revoked" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] TokenRequestDto dto)
    {
        try
        {
            var ipAddress = GetIpAddress();
            await _authService.LogoutAsync(dto.RefreshToken, ipAddress);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string userId, [FromQuery] string token)
    {
        try
        {
            var result = await _authService.VerifyEmailAsync(userId, token);
            return result
                ? Ok(new { message = "Email verified successfully" })
                : BadRequest(new { message = "Invalid or expired verification token" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        try
        {
            await _authService.ResetPasswordAsync(dto);
            return Ok(new { message = "Password reset successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("resend-verification")]
    [Authorize]
    public async Task<IActionResult> ResendVerification()
    {
        try
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _authService.SendEmailVerificationAsync(userId);
            return result
                ? Ok(new { message = "Verification email sent" })
                : BadRequest(new { message = "Failed to send verification email" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

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

    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"].ToString();
        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
    }
}
