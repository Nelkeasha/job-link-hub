using JobLinkHub.Data.Entities;

namespace JobLinkHub.Data.Repositories.Interfaces;

public interface IUserProfileRepository
{
    Task<JobSeekerProfile?> GetCandidateByUserIdAsync(long userId);
    Task<JobSeekerProfile?> GetCandidateByIdAsync(long id);
    Task<IEnumerable<JobSeekerProfile>> GetAllCandidatesAsync(string? keyword, string? location);
    Task<JobSeekerProfile> CreateCandidateAsync(JobSeekerProfile profile);
    Task<JobSeekerProfile> UpdateCandidateAsync(JobSeekerProfile profile);

    Task<EmployerProfile?> GetEmployerByUserIdAsync(long userId);
    Task<EmployerProfile?> GetEmployerByIdAsync(long id);
    Task<IEnumerable<EmployerProfile>> GetAllEmployersAsync(string? keyword, string? location);
    Task<EmployerProfile> UpdateEmployerAsync(EmployerProfile profile);

    Task UpdateJobSeekerSkillsAsync(long profileId, IEnumerable<long> skillIds);
}
