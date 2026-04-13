using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Services.Interfaces;

public interface ISavedJobService
{
    Task<IEnumerable<SavedJobDto>> GetByJobSeekerAsync(long jobSeekerProfileId);
    Task<SavedJobDto> SaveJobAsync(long jobSeekerProfileId, SaveJobDto dto);
    Task<bool> UnsaveJobAsync(long jobSeekerProfileId, long opportunityId);
    Task<bool> IsSavedAsync(long jobSeekerProfileId, long opportunityId);
    Task<int> GetCountAsync(long jobSeekerProfileId);
}