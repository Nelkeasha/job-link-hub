using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.Data.Repositories;

public class OpportunityRepository : Repository<Opportunity>, IOpportunityRepository
{
    public OpportunityRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Opportunity>> GetAllWithDetailsAsync()
        => await _context.Opportunities
            .Include(o => o.EmployerProfile)
            .Include(o => o.RequiredSkills)
                .ThenInclude(os => os.Skill)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<Opportunity?> GetByIdWithDetailsAsync(long id)
        => await _context.Opportunities
            .Include(o => o.EmployerProfile)
            .Include(o => o.RequiredSkills)
                .ThenInclude(os => os.Skill)
            .Include(o => o.Applications)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IEnumerable<Opportunity>> GetByEmployerAsync(long employerProfileId)
        => await _context.Opportunities
            .Include(o => o.RequiredSkills)
                .ThenInclude(os => os.Skill)
            .Where(o => o.EmployerProfileId == employerProfileId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Opportunity>> SearchAsync(
        string? keyword,
        string? type,
        string? location,
        string? status)
    {
        var query = _context.Opportunities
            .Include(o => o.EmployerProfile)
            .Include(o => o.RequiredSkills)
                .ThenInclude(os => os.Skill)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(o =>
                o.Title.Contains(keyword) ||
                o.Description.Contains(keyword));

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(o => o.OpportunityType == type);

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(o => o.Location != null &&
                o.Location.Contains(location));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status == status);
        else
            query = query.Where(o => o.Status == "Active");

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task IncrementViewsAsync(long id)
    {
        var opportunity = await _dbSet.FindAsync(id);
        if (opportunity != null)
        {
            opportunity.Views++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetTotalCountAsync()
        => await _context.Opportunities.CountAsync();
}