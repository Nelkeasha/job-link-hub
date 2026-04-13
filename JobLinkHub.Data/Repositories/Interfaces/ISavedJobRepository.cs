using JobLinkHub.Data.Entities;

namespace JobLinkHub.Data.Repositories.Interfaces;

public interface ISavedJobRepository : IRepository<SavedJob>
{
    Task<IEnumerable<SavedJob>> GetByJobSeekerAsync(long jobSeekerProfileId);
    Task<bool> IsSavedAsync(long jobSeekerProfileId, long opportunityId);
    Task<SavedJob?> GetByJobSeekerAndOpportunityAsync(long jobSeekerProfileId, long opportunityId);
    Task<int> GetCountByJobSeekerAsync(long jobSeekerProfileId);
}