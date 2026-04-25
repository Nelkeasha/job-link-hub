using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Services.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<UserListDto>> GetAllUsersAsync(string? role, string? keyword);
    Task<UserListDto?> GetUserByIdAsync(long id);
    Task<bool> SetUserActiveStatusAsync(long userId, bool isActive);
    Task<bool> DeleteUserAsync(long userId);
    Task<IEnumerable<AdminOpportunityDto>> GetAllOpportunitiesForModerationAsync();
    Task<bool> DeleteOpportunityAsync(long opportunityId);
    Task<bool> ToggleOpportunityStatusAsync(long opportunityId, string currentStatus);
}
