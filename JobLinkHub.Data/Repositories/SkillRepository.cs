using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.Data.Repositories;

public class SkillRepository : Repository<Skill>, ISkillRepository
{
    public SkillRepository(AppDbContext context) : base(context) { }

    public new async Task<IEnumerable<Skill>> GetAllAsync()
        => await _context.Skills.OrderBy(s => s.Category).ThenBy(s => s.Name).ToListAsync();

    public async Task<IEnumerable<string>> GetCategoriesAsync()
        => await _context.Skills
            .Where(s => s.Category != null)
            .Select(s => s.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

    public async Task<IEnumerable<Skill>> GetByCategoryAsync(string category)
        => await _context.Skills
            .Where(s => s.Category == category)
            .OrderBy(s => s.Name)
            .ToListAsync();

    public async Task<Skill?> GetByNameAsync(string name)
        => await _context.Skills.FirstOrDefaultAsync(s => s.Name == name);
}
