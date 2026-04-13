using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Services.Interfaces;

public interface IOpportunityService
{
    Task<IEnumerable<OpportunityDto>> GetAllAsync(
        string? keyword,
        string? type,
        string? location,
        string? status);
    Task<OpportunityDto?> GetByIdAsync(long id);
    Task<IEnumerable<OpportunityDto>> GetByEmployerAsync(long employerProfileId);
    Task<OpportunityDto> CreateAsync(long employerProfileId, CreateOpportunityDto dto);
    Task<OpportunityDto?> UpdateAsync(long id, long employerProfileId, UpdateOpportunityDto dto);
    Task<bool> DeleteAsync(long id, long employerProfileId);
    Task IncrementViewsAsync(long id);
    Task<int> GetTotalCountAsync();
}