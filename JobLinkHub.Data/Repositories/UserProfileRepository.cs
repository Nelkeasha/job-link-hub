using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.Data.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly AppDbContext _context;
    public UserProfileRepository(AppDbContext context) { _context = context; }

    public async Task<JobSeekerProfile?> GetCandidateByUserIdAsync(long userId)
        => await _context.JobSeekerProfiles
            .Include(p => p.User)
            .Include(p => p.Skills).ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task<JobSeekerProfile?> GetCandidateByIdAsync(long id)
        => await _context.JobSeekerProfiles
            .Include(p => p.User)
            .Include(p => p.Skills).ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<JobSeekerProfile>> GetAllCandidatesAsync(string? keyword, string? location)
    {
        var query = _context.JobSeekerProfiles
            .Include(p => p.User)
            .Include(p => p.Skills).ThenInclude(s => s.Skill)
            .Where(p => p.User.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(p =>
                (p.Bio != null && p.Bio.Contains(keyword)) ||
                p.User.FirstName.Contains(keyword) ||
                p.User.LastName.Contains(keyword) ||
                p.Skills.Any(s => s.Skill.Name.Contains(keyword)));

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(p => p.Location != null && p.Location.Contains(location));

        return await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<JobSeekerProfile> CreateCandidateAsync(JobSeekerProfile profile)
    {
        await _context.JobSeekerProfiles.AddAsync(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<JobSeekerProfile> UpdateCandidateAsync(JobSeekerProfile profile)
    {
        _context.JobSeekerProfiles.Update(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<EmployerProfile?> GetEmployerByUserIdAsync(long userId)
        => await _context.EmployerProfiles
            .Include(p => p.User)
            .Include(p => p.Opportunities.Where(o => o.Status == "Active"))
            .FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task<EmployerProfile?> GetEmployerByIdAsync(long id)
        => await _context.EmployerProfiles
            .Include(p => p.User)
            .Include(p => p.Opportunities)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<EmployerProfile>> GetAllEmployersAsync(string? keyword, string? location)
    {
        var query = _context.EmployerProfiles
            .Include(p => p.User)
            .Include(p => p.Opportunities.Where(o => o.Status == "Active"))
            .Where(p => p.User.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(p =>
                p.CompanyName.Contains(keyword) ||
                (p.CompanyDescription != null && p.CompanyDescription.Contains(keyword)) ||
                (p.Industry != null && p.Industry.Contains(keyword)));

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(p => p.Location != null && p.Location.Contains(location));

        return await query.OrderBy(p => p.CompanyName).ToListAsync();
    }

    public async Task<EmployerProfile> UpdateEmployerAsync(EmployerProfile profile)
    {
        _context.EmployerProfiles.Update(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task UpdateJobSeekerSkillsAsync(long profileId, IEnumerable<long> skillIds)
    {
        var existing = await _context.JobSeekerSkills
            .Where(js => js.JobSeekerProfileId == profileId)
            .ToListAsync();
        _context.JobSeekerSkills.RemoveRange(existing);

        foreach (var skillId in skillIds)
        {
            await _context.JobSeekerSkills.AddAsync(new JobSeekerSkill
            {
                JobSeekerProfileId = profileId,
                SkillId = skillId
            });
        }
        await _context.SaveChangesAsync();
    }
}
