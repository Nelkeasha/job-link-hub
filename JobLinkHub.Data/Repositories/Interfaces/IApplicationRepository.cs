using JobLinkHub.Data.Entities;

namespace JobLinkHub.Data.Repositories.Interfaces;

public interface IApplicationRepository : IRepository<Application>
{
    Task<IEnumerable<Application>> GetByOpportunityAsync(long opportunityId);
    Task<IEnumerable<Application>> GetByEmployerAsync(long employerProfileId);
    Task<IEnumerable<Application>> GetByJobSeekerAsync(long jobSeekerProfileId);
    Task<Application?> GetByIdWithDetailsAsync(long id);
    Task<bool> HasAppliedAsync(long jobSeekerProfileId, long opportunityId);
    Task<int> GetCountByOpportunityAsync(long opportunityId);
}