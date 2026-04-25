using JobLinkHub.Data.Entities;

namespace JobLinkHub.Data.Repositories.Interfaces;

public interface ISkillRepository : IRepository<Skill>
{
    new Task<IEnumerable<Skill>> GetAllAsync();
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<IEnumerable<Skill>> GetByCategoryAsync(string category);
    Task<Skill?> GetByNameAsync(string name);
}
