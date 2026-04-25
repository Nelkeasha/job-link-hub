using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Services.Interfaces;

public interface IApplicationService
{
    Task<IEnumerable<ApplicationDto>> GetByOpportunityAsync(long opportunityId);
    Task<IEnumerable<ApplicationDto>> GetByEmployerAsync(long employerProfileId);
    Task<IEnumerable<ApplicationDto>> GetByJobSeekerAsync(long jobSeekerProfileId);
    Task<ApplicationDto?> GetByIdAsync(long id);
    Task<ApplicationDto> CreateAsync(long jobSeekerProfileId, CreateApplicationDto dto);
    Task<ApplicationDto?> UpdateStatusAsync(long id, UpdateApplicationStatusDto dto);
    Task<bool> WithdrawAsync(long id, long jobSeekerProfileId);
    Task<bool> HasAppliedAsync(long jobSeekerProfileId, long opportunityId);
}