using JobLinkHub.Data;
using JobLinkHub.Data.Entities;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobLinkHub.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _config;
    private readonly AppDbContext _context;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration config,
        AppDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _config = config;
        _context = context;
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
            EmailConfirmed = true,
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
            EmailConfirmed = true,
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

        return await GenerateAuthResponseAsync(user, profile.Id);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new Exception("Invalid email or password");

        if (!user.IsActive)
            throw new Exception("Account is deactivated");

        var result = await _signInManager
            .CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
            throw new Exception("Account locked. Try again later");

        if (!result.Succeeded)
            throw new Exception("Invalid email or password");

        long? profileId = null;
        if (user.Role == "CANDIDATE")
        {
            var profile = _context.JobSeekerProfiles
                .FirstOrDefault(p => p.UserId == user.Id);
            profileId = profile?.Id;
        }
        else if (user.Role == "EMPLOYER")
        {
            var profile = _context.EmployerProfiles
                .FirstOrDefault(p => p.UserId == user.Id);
            profileId = profile?.Id;
        }

        return await GenerateAuthResponseAsync(user, profileId);
    }

    public async Task<bool> ChangePasswordAsync(long userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new Exception("User not found");

        var result = await _userManager
            .ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        return result.Succeeded;
    }

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(User user, long? profileId)
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
            Email = user.Email!,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = user.Role,
            UserId = user.Id,
            ProfileId = profileId,
            ExpiresAt = expiration
        };
    }
}