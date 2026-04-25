using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;

namespace JobLinkHub.Services.Implementations;

public class SkillService : ISkillService
{
    private readonly ISkillRepository _repo;
    public SkillService(ISkillRepository repo) { _repo = repo; }

    public async Task<IEnumerable<SkillDto>> GetAllAsync()
    {
        var skills = await _repo.GetAllAsync();
        return skills.Select(Map);
    }

    public async Task<IEnumerable<SkillCategoryDto>> GetGroupedByCategoryAsync()
    {
        var skills = await _repo.GetAllAsync();
        return skills
            .GroupBy(s => s.Category ?? "Other")
            .Select(g => new SkillCategoryDto
            {
                Category = g.Key,
                Skills = g.Select(Map).ToList()
            })
            .OrderBy(g => g.Category)
            .ToList();
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
        => await _repo.GetCategoriesAsync();

    public async Task<IEnumerable<SkillDto>> GetByCategoryAsync(string category)
    {
        var skills = await _repo.GetByCategoryAsync(category);
        return skills.Select(Map);
    }

    public async Task<SkillDto?> GetByIdAsync(long id)
    {
        var skill = await _repo.GetByIdAsync(id);
        return skill == null ? null : Map(skill);
    }

    public async Task<SkillDto> CreateAsync(CreateSkillDto dto)
    {
        var skill = new Skill { Name = dto.Name, Category = dto.Category };
        await _repo.AddAsync(skill);
        await _repo.SaveChangesAsync();
        return Map(skill);
    }

    public async Task<SkillDto?> UpdateAsync(long id, UpdateSkillDto dto)
    {
        var skill = await _repo.GetByIdAsync(id);
        if (skill == null) return null;

        skill.Name = dto.Name;
        skill.Category = dto.Category;
        await _repo.UpdateAsync(skill);
        await _repo.SaveChangesAsync();
        return Map(skill);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var skill = await _repo.GetByIdAsync(id);
        if (skill == null) return false;

        await _repo.DeleteAsync(skill);
        await _repo.SaveChangesAsync();
        return true;
    }

    private static SkillDto Map(Skill s) => new() { Id = s.Id, Name = s.Name, Category = s.Category };
}
