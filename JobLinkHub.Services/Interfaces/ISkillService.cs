using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Services.Interfaces;

public interface ISkillService
{
    Task<IEnumerable<SkillDto>> GetAllAsync();
    Task<IEnumerable<SkillCategoryDto>> GetGroupedByCategoryAsync();
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<IEnumerable<SkillDto>> GetByCategoryAsync(string category);
    Task<SkillDto?> GetByIdAsync(long id);
    Task<SkillDto> CreateAsync(CreateSkillDto dto);
    Task<SkillDto?> UpdateAsync(long id, UpdateSkillDto dto);
    Task<bool> DeleteAsync(long id);
}
