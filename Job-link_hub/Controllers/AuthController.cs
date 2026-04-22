using JobLinkHub.API.Models.Auth;
using JobLinkHub.API.Services;
using JobLinkHub.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IJwtTokenService jwtTokenService) : ControllerBase
{
    private const string TokenProvider = "JobLinkHub";
    private const string RefreshTokenName = "refresh_token";

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var role = request.Role.ToUpperInvariant();
        if (role is not ("CANDIDATE" or "EMPLOYER"))
        {
            return BadRequest("Role must be CANDIDATE or EMPLOYER.");
        }

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Conflict("Email is already registered.");
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = role
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(createResult.Errors);
        }

        await userManager.AddToRoleAsync(user, role);
        var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(emailToken);

        return Ok(new
        {
            message = "Registration successful. Verify email with the token below.",
            emailVerification = new
            {
                userId = user.Id,
                token = encodedToken
            }
        });
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] long userId, [FromQuery] string token)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return NotFound("User not found.");

        var decodedToken = WebUtility.UrlDecode(token);
        var result = await userManager.ConfirmEmailAsync(user, decodedToken!);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok("Email verified successfully.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return Unauthorized("Invalid credentials.");

        var passwordResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!passwordResult.Succeeded) return Unauthorized("Invalid credentials.");

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();
        await userManager.SetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenName, refreshToken);

        var expiresAt = DateTime.UtcNow.AddMinutes(60);
        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAtUtc = expiresAt
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return Unauthorized();

        var storedToken = await userManager.GetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenName);
        if (storedToken != request.RefreshToken) return Unauthorized("Invalid refresh token.");

        var newRefreshToken = jwtTokenService.GenerateRefreshToken();
        await userManager.SetAuthenticationTokenAsync(user, TokenProvider, RefreshTokenName, newRefreshToken);

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new AuthResponse
        {
            AccessToken = jwtTokenService.GenerateAccessToken(user, roles),
            RefreshToken = newRefreshToken,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60)
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return Unauthorized();

        await userManager.RemoveAuthenticationTokenAsync(user, TokenProvider, RefreshTokenName);
        await signInManager.SignOutAsync();
        return Ok("Logged out successfully.");
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Ok("If the account exists, a reset token has been generated.");
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return Ok(new
        {
            message = "Password reset token generated.",
            email = user.Email,
            token = WebUtility.UrlEncode(token)
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return BadRequest("Invalid reset request.");

        var token = WebUtility.UrlDecode(request.Token);
        var result = await userManager.ResetPasswordAsync(user, token!, request.NewPassword);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok("Password reset successful.");
    }
}
