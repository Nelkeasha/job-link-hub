using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;

namespace JobLinkHub.Services.Implementations;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _repo;

    public ApplicationService(IApplicationRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<ApplicationDto>> GetByOpportunityAsync(long opportunityId)
    {
        var applications = await _repo.GetByOpportunityAsync(opportunityId);
        return applications.Select(MapToDto);
    }

    public async Task<IEnumerable<ApplicationDto>> GetByJobSeekerAsync(long jobSeekerProfileId)
    {
        var applications = await _repo.GetByJobSeekerAsync(jobSeekerProfileId);
        return applications.Select(MapToDto);
    }

    public async Task<ApplicationDto?> GetByIdAsync(long id)
    {
        var application = await _repo.GetByIdWithDetailsAsync(id);
        return application == null ? null : MapToDto(application);
    }

    public async Task<ApplicationDto> CreateAsync(
        long jobSeekerProfileId, CreateApplicationDto dto)
    {
        var application = new Application
        {
            OpportunityId = dto.OpportunityId,
            JobSeekerProfileId = jobSeekerProfileId,
            CoverLetter = dto.CoverLetter,
            ResumeUsed = dto.ResumeUsed,
            Status = "PENDING",
            ApplicationDate = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(application);
        await _repo.SaveChangesAsync();

        var created = await _repo.GetByIdWithDetailsAsync(application.Id);
        return MapToDto(created!);
    }

    public async Task<ApplicationDto?> UpdateStatusAsync(
        long id, UpdateApplicationStatusDto dto)
    {
        var application = await _repo.GetByIdWithDetailsAsync(id);
        if (application == null) return null;

        application.Status = dto.Status;
        application.RejectionReason = dto.RejectionReason;
        application.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(application);
        await _repo.SaveChangesAsync();

        return MapToDto(application);
    }

    public async Task<bool> WithdrawAsync(long id, long jobSeekerProfileId)
    {
        var application = await _repo.GetByIdAsync(id);

        if (application == null || application.JobSeekerProfileId != jobSeekerProfileId)
            return false;

        application.Status = "WITHDRAWN";
        application.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(application);
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasAppliedAsync(long jobSeekerProfileId, long opportunityId)
        => await _repo.HasAppliedAsync(jobSeekerProfileId, opportunityId);

    private static ApplicationDto MapToDto(Application a) => new()
    {
        Id = a.Id,
        OpportunityId = a.OpportunityId,
        OpportunityTitle = a.Opportunity?.Title ?? string.Empty,
        CompanyName = a.Opportunity?.EmployerProfile?.CompanyName ?? string.Empty,
        JobSeekerProfileId = a.JobSeekerProfileId,
        CandidateName = a.JobSeekerProfile?.User != null
            ? $"{a.JobSeekerProfile.User.FirstName} {a.JobSeekerProfile.User.LastName}"
            : string.Empty,
        CandidateEmail = a.JobSeekerProfile?.User?.Email ?? string.Empty,
        Status = a.Status,
        CoverLetter = a.CoverLetter,
        ResumeUsed = a.ResumeUsed,
        RejectionReason = a.RejectionReason,
        ApplicationDate = a.ApplicationDate,
        UpdatedAt = a.UpdatedAt
    };
}