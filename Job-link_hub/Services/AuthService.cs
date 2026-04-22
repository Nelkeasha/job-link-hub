using JobLinkHub.Data;
using JobLinkHub.Data.Entities;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JobLinkHub.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration config,
        AppDbContext context,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _context = context;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> RegisterCandidateAsync(RegisterCandidateDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            throw new Exception("Email already registered");

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            Role = "CANDIDATE",
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ",
                result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "CANDIDATE");

        var profile = new JobSeekerProfile
        {
            UserId = user.Id,
            Location = dto.Location,
            CareerInterest = dto.CareerInterest,
            CreatedAt = DateTime.UtcNow
        };
        _context.JobSeekerProfiles.Add(profile);
        await _context.SaveChangesAsync();

        await SendEmailVerificationAsync(user.Id);

        return await GenerateAuthResponseAsync(user, profile.Id);
    }

    public async Task<AuthResponseDto> RegisterEmployerAsync(RegisterEmployerDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            throw new Exception("Email already registered");

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Role = "EMPLOYER",
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ",
                result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "EMPLOYER");

        var profile = new EmployerProfile
        {
            UserId = user.Id,
            CompanyName = dto.CompanyName,
            Industry = dto.Industry,
            Website = dto.Website,
            Location = dto.Location,
            CompanyDescription = dto.CompanyDescription,
            CreatedAt = DateTime.UtcNow
        };
        _context.EmployerProfiles.Add(profile);
        await _context.SaveChangesAsync();

        await SendEmailVerificationAsync(user.Id);

        return await GenerateAuthResponseAsync(user, profile.Id);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, string? ipAddress = null)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new Exception("Invalid email or password");

        if (!user.IsActive)
            throw new Exception("Account is deactivated");

        if (!user.EmailConfirmed)
            throw new Exception("Email not verified. Please check your inbox for the verification link.");

        var result = await _signInManager
            .CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
            throw new Exception("Account locked. Try again later");

        if (!result.Succeeded)
            throw new Exception("Invalid email or password");

        long? profileId = null;
        if (user.Role == "CANDIDATE")
        {
            var profile = await _context.JobSeekerProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
            profileId = profile?.Id;
        }
        else if (user.Role == "EMPLOYER")
        {
            var profile = await _context.EmployerProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
            profileId = profile?.Id;
        }

        return await GenerateAuthResponseAsync(user, profileId, ipAddress);
    }

    public async Task<bool> ChangePasswordAsync(long userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new Exception("User not found");

        var result = await _userManager
            .ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        return result.Succeeded;
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken)
            ?? throw new Exception("Invalid refresh token");

        if (token.IsRevoked)
        {
            // Possible token reuse detected — revoke all descendant tokens
            await RevokeDescendantTokensAsync(token, ipAddress, $"Attempted reuse of revoked ancestor token: {token.Token}");
            throw new Exception("Refresh token has been revoked");
        }

        if (token.IsExpired)
            throw new Exception("Refresh token has expired");

        // Rotate: revoke current, create new
        var newRefreshToken = GenerateRefreshToken(token.User.Id, ipAddress);
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReplacedByToken = newRefreshToken.Token;

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        var user = token.User;
        long? profileId = null;
        if (user.Role == "CANDIDATE")
        {
            var profile = await _context.JobSeekerProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
            profileId = profile?.Id;
        }
        else if (user.Role == "EMPLOYER")
        {
            var profile = await _context.EmployerProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
            profileId = profile?.Id;
        }

        return await GenerateJwtResponseAsync(user, profileId, newRefreshToken);
    }

    public async Task RevokeTokenAsync(string refreshToken, string ipAddress)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken)
            ?? throw new Exception("Invalid refresh token");

        if (!token.IsActive)
            throw new Exception("Token is already inactive");

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        await _context.SaveChangesAsync();
    }

    public async Task LogoutAsync(string refreshToken, string ipAddress)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token != null && token.IsActive)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> SendEmailVerificationAsync(long userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var frontendUrl = _config["Email:FrontendBaseUrl"] ?? "http://localhost:3000";
        var verificationLink = $"{frontendUrl}/verify-email?userId={user.Id}&token={encodedToken}";

        await _emailService.SendEmailVerificationAsync(
            user.Email!,
            $"{user.FirstName} {user.LastName}",
            verificationLink);

        return true;
    }

    public async Task<bool> VerifyEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var decodedToken = Uri.UnescapeDataString(token);
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        return result.Succeeded;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return true; // Don't reveal if email exists

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var frontendUrl = _config["Email:FrontendBaseUrl"] ?? "http://localhost:3000";
        var resetLink = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={encodedToken}";

        await _emailService.SendPasswordResetAsync(
            user.Email!,
            $"{user.FirstName} {user.LastName}",
            resetLink);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new Exception("User not found");

        var decodedToken = Uri.UnescapeDataString(dto.Token);
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, dto.NewPassword);

        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        return true;
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user, long? profileId, string? ipAddress = null)
    {
        var refreshToken = GenerateRefreshToken(user.Id, ipAddress);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return await GenerateJwtResponseAsync(user, profileId, refreshToken);
    }

    private async Task<AuthResponseDto> GenerateJwtResponseAsync(User user, long? profileId, RefreshToken refreshToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var expiration = DateTime.UtcNow.AddMinutes(
            double.Parse(_config["Jwt:ExpirationMinutes"] ?? "60"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email,          user.Email!),
            new(ClaimTypes.GivenName,      user.FirstName),
            new(ClaimTypes.Surname,        user.LastName),
            new("ProfileId",               profileId?.ToString() ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: creds);

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            Email = user.Email!,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = user.Role,
            UserId = user.Id,
            ProfileId = profileId,
            ExpiresAt = expiration
        };
    }

    private static RefreshToken GenerateRefreshToken(long userId, string? ipAddress)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }

    private async Task RevokeDescendantTokensAsync(RefreshToken token, string ipAddress, string reason)
    {
        if (!string.IsNullOrEmpty(token.ReplacedByToken))
        {
            var childToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token.ReplacedByToken);

            if (childToken != null)
            {
                if (childToken.IsActive)
                {
                    childToken.RevokedAt = DateTime.UtcNow;
                    childToken.RevokedByIp = ipAddress;
                }
                await RevokeDescendantTokensAsync(childToken, ipAddress, reason);
            }
        }

        await _context.SaveChangesAsync();
    }
}
