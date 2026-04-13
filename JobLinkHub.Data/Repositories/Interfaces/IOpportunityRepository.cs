using JobLinkHub.Data.Entities;

namespace JobLinkHub.Data.Repositories.Interfaces;

public interface IOpportunityRepository : IRepository<Opportunity>
{
    Task<IEnumerable<Opportunity>> GetAllWithDetailsAsync();
    Task<Opportunity?> GetByIdWithDetailsAsync(long id);
    Task<IEnumerable<Opportunity>> GetByEmployerAsync(long employerProfileId);
    Task<IEnumerable<Opportunity>> SearchAsync(
        string? keyword,
        string? type,
        string? location,
        string? status);
    Task IncrementViewsAsync(long id);
    Task<int> GetTotalCountAsync();
}