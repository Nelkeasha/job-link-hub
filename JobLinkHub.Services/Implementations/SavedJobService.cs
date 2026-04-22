using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;

namespace JobLinkHub.Services.Implementations;

public class SavedJobService : ISavedJobService
{
    private readonly ISavedJobRepository _repo;

    public SavedJobService(ISavedJobRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<SavedJobDto>> GetByJobSeekerAsync(long jobSeekerProfileId)
    {
        var savedJobs = await _repo.GetByJobSeekerAsync(jobSeekerProfileId);
        return savedJobs.Select(MapToDto);
    }

    public async Task<SavedJobDto> SaveJobAsync(long jobSeekerProfileId, SaveJobDto dto)
    {
        var savedJob = new SavedJob
        {
            JobSeekerProfileId = jobSeekerProfileId,
            OpportunityId = dto.OpportunityId,
            Notes = dto.Notes,
            SavedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(savedJob);
        await _repo.SaveChangesAsync();

        var created = await _repo.GetByJobSeekerAndOpportunityAsync(
            jobSeekerProfileId, dto.OpportunityId);
        return MapToDto(created!);
    }

    public async Task<bool> UnsaveJobAsync(long jobSeekerProfileId, long opportunityId)
    {
        var savedJob = await _repo.GetByJobSeekerAndOpportunityAsync(
            jobSeekerProfileId, opportunityId);

        if (savedJob == null) return false;

        await _repo.DeleteAsync(savedJob);
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsSavedAsync(long jobSeekerProfileId, long opportunityId)
        => await _repo.IsSavedAsync(jobSeekerProfileId, opportunityId);

    public async Task<int> GetCountAsync(long jobSeekerProfileId)
        => await _repo.GetCountByJobSeekerAsync(jobSeekerProfileId);

    private static SavedJobDto MapToDto(SavedJob s) => new()
    {
        Id = s.Id,
        OpportunityId = s.OpportunityId,
        Title = s.Opportunity?.Title ?? string.Empty,
        CompanyName = s.Opportunity?.EmployerProfile?.CompanyName ?? string.Empty,
        Location = s.Opportunity?.Location,
        OpportunityType = s.Opportunity?.OpportunityType ?? string.Empty,
        SalaryRange = s.Opportunity?.SalaryRange,
        Deadline = s.Opportunity?.Deadline,
        SavedAt = s.SavedAt,
        Notes = s.Notes
    };
}