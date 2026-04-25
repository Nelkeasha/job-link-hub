using JobLinkHub.Data;
using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.Services.Implementations;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _profileRepo;
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;

    public UserProfileService(
        IUserProfileRepository profileRepo,
        UserManager<User> userManager,
        AppDbContext context)
    {
        _profileRepo = profileRepo;
        _userManager = userManager;
        _context = context;
    }

    public async Task<CandidateProfileDto?> GetCandidateByUserIdAsync(long userId)
    {
        var profile = await _profileRepo.GetCandidateByUserIdAsync(userId);
        return profile == null ? null : MapCandidate(profile);
    }

    public async Task<CandidateProfileDto?> GetCandidateByIdAsync(long id)
    {
        var profile = await _profileRepo.GetCandidateByIdAsync(id);
        return profile == null ? null : MapCandidate(profile);
    }

    public async Task<IEnumerable<CandidateProfileDto>> GetAllCandidatesAsync(string? keyword, string? location)
    {
        var profiles = await _profileRepo.GetAllCandidatesAsync(keyword, location);
        return profiles.Select(MapCandidate);
    }

    public async Task<CandidateProfileDto> UpdateCandidateAsync(long userId, UpdateCandidateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new Exception("User not found");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var profile = await _profileRepo.GetCandidateByUserIdAsync(userId);
        if (profile == null)
        {
            profile = new JobSeekerProfile { UserId = userId };
            await _profileRepo.CreateCandidateAsync(profile);
        }

        profile.Bio = dto.Bio;
        profile.CareerInterest = dto.CareerInterest;
        profile.EducationLevel = dto.EducationLevel;
        profile.Institution = dto.Institution;
        profile.GraduationYear = dto.GraduationYear;
        profile.ResumeUrl = dto.ResumeUrl;
        profile.PortfolioUrl = dto.PortfolioUrl;
        profile.LinkedInUrl = dto.LinkedInUrl;
        profile.Location = dto.Location;

        await _profileRepo.UpdateCandidateAsync(profile);

        if (dto.SkillIds.Any())
            await _profileRepo.UpdateJobSeekerSkillsAsync(profile.Id, dto.SkillIds);

        return MapCandidate(await _profileRepo.GetCandidateByUserIdAsync(userId) ?? profile);
    }

    public async Task<EmployerProfileDto?> GetEmployerByUserIdAsync(long userId)
    {
        var profile = await _profileRepo.GetEmployerByUserIdAsync(userId);
        return profile == null ? null : MapEmployer(profile);
    }

    public async Task<EmployerProfileDto?> GetEmployerByIdAsync(long id)
    {
        var profile = await _profileRepo.GetEmployerByIdAsync(id);
        return profile == null ? null : MapEmployer(profile);
    }

    public async Task<IEnumerable<EmployerProfileDto>> GetAllEmployersAsync(string? keyword, string? location)
    {
        var profiles = await _profileRepo.GetAllEmployersAsync(keyword, location);
        return profiles.Select(MapEmployer);
    }

    public async Task<EmployerProfileDto> UpdateEmployerAsync(long userId, UpdateEmployerProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new Exception("User not found");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var profile = await _profileRepo.GetEmployerByUserIdAsync(userId)
            ?? throw new Exception("Employer profile not found");

        profile.CompanyName = dto.CompanyName;
        profile.CompanyType = dto.CompanyType;
        profile.Industry = dto.Industry;
        profile.CompanyDescription = dto.CompanyDescription;
        profile.Website = dto.Website;
        profile.Location = dto.Location;
        profile.LogoUrl = dto.LogoUrl;

        await _profileRepo.UpdateEmployerAsync(profile);
        return MapEmployer(profile);
    }

    private static CandidateProfileDto MapCandidate(JobSeekerProfile p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        FirstName = p.User.FirstName,
        LastName = p.User.LastName,
        Email = p.User.Email ?? string.Empty,
        PhoneNumber = p.User.PhoneNumber,
        ProfilePictureUrl = p.User.ProfilePictureUrl,
        Bio = p.Bio,
        CareerInterest = p.CareerInterest,
        EducationLevel = p.EducationLevel,
        Institution = p.Institution,
        GraduationYear = p.GraduationYear,
        ResumeUrl = p.ResumeUrl,
        PortfolioUrl = p.PortfolioUrl,
        LinkedInUrl = p.LinkedInUrl,
        Location = p.Location,
        CreatedAt = p.CreatedAt,
        Skills = p.Skills.Select(s => s.Skill.Name).ToList(),
        SkillIds = p.Skills.Select(s => s.SkillId).ToList()
    };

    private static EmployerProfileDto MapEmployer(EmployerProfile p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        FirstName = p.User.FirstName,
        LastName = p.User.LastName,
        Email = p.User.Email ?? string.Empty,
        PhoneNumber = p.User.PhoneNumber,
        CompanyName = p.CompanyName,
        CompanyType = p.CompanyType,
        Industry = p.Industry,
        CompanyDescription = p.CompanyDescription,
        Website = p.Website,
        Location = p.Location,
        LogoUrl = p.LogoUrl,
        ActiveOpportunitiesCount = p.Opportunities.Count(o => o.Status == "Active"),
        CreatedAt = p.CreatedAt
    };
}
