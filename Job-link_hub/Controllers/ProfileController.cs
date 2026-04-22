using JobLinkHub.API.Models.Profile;
using JobLinkHub.API.Services;
using JobLinkHub.Data;
using JobLinkHub.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public class ProfileController(
    AppDbContext dbContext,
    IUserContextService userContextService,
    INotificationService notificationService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var user = await userContextService.GetCurrentUserAsync(User);
        if (user is null) return Unauthorized();

        var userDto = ToUserSummary(user);
        if (string.Equals(user.Role, "EMPLOYER", StringComparison.OrdinalIgnoreCase))
        {
            var employerProfile = await dbContext.EmployerProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
            return Ok(new
            {
                user = userDto,
                employerProfile = employerProfile is null ? null : ToEmployerProfileDto(employerProfile)
            });
        }

        var candidateProfile = await dbContext.JobSeekerProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
        return Ok(new
        {
            user = userDto,
            candidateProfile = candidateProfile is null ? null : ToCandidateProfileDto(candidateProfile)
        });
    }

    [HttpPut("candidate")]
    public async Task<IActionResult> UpsertCandidateProfile(UpdateCandidateProfileRequest request)
    {
        var user = await userContextService.GetCurrentUserAsync(User);
        if (user is null) return Unauthorized();
        if (!string.Equals(user.Role, "CANDIDATE", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var profile = await dbContext.JobSeekerProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (profile is null)
        {
            profile = new JobSeekerProfile { UserId = user.Id };
            dbContext.JobSeekerProfiles.Add(profile);
        }

        profile.Bio = request.Bio;
        profile.CareerInterest = request.CareerInterest;
        profile.EducationLevel = request.EducationLevel;
        profile.Institution = request.Institution;
        profile.GraduationYear = request.GraduationYear;
        profile.ResumeUrl = request.ResumeUrl;
        profile.PortfolioUrl = request.PortfolioUrl;
        profile.LinkedInUrl = request.LinkedInUrl;
        profile.Location = request.Location;

        await dbContext.SaveChangesAsync();
        await notificationService.CreateAsync(user.Id, "Your candidate profile was updated.", "PROFILE");
        return Ok(ToCandidateProfileDto(profile));
    }

    [HttpPut("employer")]
    public async Task<IActionResult> UpsertEmployerProfile(UpdateEmployerProfileRequest request)
    {
        var user = await userContextService.GetCurrentUserAsync(User);
        if (user is null) return Unauthorized();
        if (!string.Equals(user.Role, "EMPLOYER", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var profile = await dbContext.EmployerProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (profile is null)
        {
            profile = new EmployerProfile { UserId = user.Id };
            dbContext.EmployerProfiles.Add(profile);
        }

        profile.CompanyName = request.CompanyName;
        profile.CompanyType = request.CompanyType;
        profile.Industry = request.Industry;
        profile.CompanyDescription = request.CompanyDescription;
        profile.Website = request.Website;
        profile.Location = request.Location;
        profile.LogoUrl = request.LogoUrl;

        await dbContext.SaveChangesAsync();
        await notificationService.CreateAsync(user.Id, "Your employer profile was updated.", "PROFILE");
        return Ok(ToEmployerProfileDto(profile));
    }

    private static UserSummaryDto ToUserSummary(User user)
    {
        return new UserSummaryDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            ProfilePictureUrl = user.ProfilePictureUrl
        };
    }

    private static CandidateProfileDto ToCandidateProfileDto(JobSeekerProfile profile)
    {
        return new CandidateProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            Bio = profile.Bio,
            CareerInterest = profile.CareerInterest,
            EducationLevel = profile.EducationLevel,
            Institution = profile.Institution,
            GraduationYear = profile.GraduationYear,
            ResumeUrl = profile.ResumeUrl,
            PortfolioUrl = profile.PortfolioUrl,
            LinkedInUrl = profile.LinkedInUrl,
            Location = profile.Location
        };
    }

    private static EmployerProfileDto ToEmployerProfileDto(EmployerProfile profile)
    {
        return new EmployerProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            CompanyName = profile.CompanyName,
            CompanyType = profile.CompanyType,
            Industry = profile.Industry,
            CompanyDescription = profile.CompanyDescription,
            Website = profile.Website,
            Location = profile.Location,
            LogoUrl = profile.LogoUrl
        };
    }
}
