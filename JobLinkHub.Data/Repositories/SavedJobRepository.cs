using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.Data.Repositories;

public class SavedJobRepository : Repository<SavedJob>, ISavedJobRepository
{
    public SavedJobRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<SavedJob>> GetByJobSeekerAsync(long jobSeekerProfileId)
        => await _context.SavedJobs
            .Include(s => s.Opportunity)
                .ThenInclude(o => o.EmployerProfile)
            .Where(s => s.JobSeekerProfileId == jobSeekerProfileId)
            .OrderByDescending(s => s.SavedAt)
            .ToListAsync();

    public async Task<bool> IsSavedAsync(long jobSeekerProfileId, long opportunityId)
        => await _context.SavedJobs
            .AnyAsync(s =>
                s.JobSeekerProfileId == jobSeekerProfileId &&
                s.OpportunityId == opportunityId);

    public async Task<SavedJob?> GetByJobSeekerAndOpportunityAsync(
        long jobSeekerProfileId, long opportunityId)
        => await _context.SavedJobs
            .FirstOrDefaultAsync(s =>
                s.JobSeekerProfileId == jobSeekerProfileId &&
                s.OpportunityId == opportunityId);

    public async Task<int> GetCountByJobSeekerAsync(long jobSeekerProfileId)
        => await _context.SavedJobs
            .CountAsync(s => s.JobSeekerProfileId == jobSeekerProfileId);
}