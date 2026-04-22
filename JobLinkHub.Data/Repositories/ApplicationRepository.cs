using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.Data.Repositories;

public class ApplicationRepository : Repository<Application>, IApplicationRepository
{
    public ApplicationRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Application>> GetByOpportunityAsync(long opportunityId)
        => await _context.Applications
            .Include(a => a.JobSeekerProfile)
                .ThenInclude(p => p.User)
            .Include(a => a.JobSeekerProfile)
                .ThenInclude(p => p.Skills)
                    .ThenInclude(s => s.Skill)
            .Where(a => a.OpportunityId == opportunityId)
            .OrderByDescending(a => a.ApplicationDate)
            .ToListAsync();

    public async Task<IEnumerable<Application>> GetByJobSeekerAsync(long jobSeekerProfileId)
        => await _context.Applications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o.EmployerProfile)
            .Where(a => a.JobSeekerProfileId == jobSeekerProfileId)
            .OrderByDescending(a => a.ApplicationDate)
            .ToListAsync();

    public async Task<Application?> GetByIdWithDetailsAsync(long id)
        => await _context.Applications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o.EmployerProfile)
            .Include(a => a.JobSeekerProfile)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<bool> HasAppliedAsync(long jobSeekerProfileId, long opportunityId)
        => await _context.Applications
            .AnyAsync(a =>
                a.JobSeekerProfileId == jobSeekerProfileId &&
                a.OpportunityId == opportunityId);

    public async Task<int> GetCountByOpportunityAsync(long opportunityId)
        => await _context.Applications
            .CountAsync(a => a.OpportunityId == opportunityId);
}