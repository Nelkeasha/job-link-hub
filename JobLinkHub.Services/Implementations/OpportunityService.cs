using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;

namespace JobLinkHub.Services.Implementations;

public class OpportunityService : IOpportunityService
{
    private readonly IOpportunityRepository _repo;

    public OpportunityService(IOpportunityRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<OpportunityDto>> GetAllAsync(
        string? keyword, string? type, string? location, string? status)
    {
        var opportunities = await _repo.SearchAsync(keyword, type, location, status);
        return opportunities.Select(MapToDto);
    }

    public async Task<OpportunityDto?> GetByIdAsync(long id)
    {
        var opportunity = await _repo.GetByIdWithDetailsAsync(id);
        return opportunity == null ? null : MapToDto(opportunity);
    }

    public async Task<IEnumerable<OpportunityDto>> GetByEmployerAsync(long employerProfileId)
    {
        var opportunities = await _repo.GetByEmployerAsync(employerProfileId);
        return opportunities.Select(MapToDto);
    }

    public async Task<OpportunityDto> CreateAsync(long employerProfileId, CreateOpportunityDto dto)
    {
        var opportunity = new Opportunity
        {
            EmployerProfileId = employerProfileId,
            Title = dto.Title,
            Description = dto.Description,
            OpportunityType = dto.OpportunityType,
            Location = dto.Location,
            QualificationRequired = dto.QualificationRequired,
            SalaryRange = dto.SalaryRange,
            Deadline = dto.Deadline,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            RequiredSkills = dto.SkillIds.Select(skillId => new OpportunitySkill
            {
                SkillId = skillId
            }).ToList()
        };

        await _repo.AddAsync(opportunity);
        await _repo.SaveChangesAsync();

        var created = await _repo.GetByIdWithDetailsAsync(opportunity.Id);
        return MapToDto(created!);
    }

    public async Task<OpportunityDto?> UpdateAsync(
        long id, long employerProfileId, UpdateOpportunityDto dto)
    {
        var opportunity = await _repo.GetByIdWithDetailsAsync(id);

        if (opportunity == null || opportunity.EmployerProfileId != employerProfileId)
            return null;

        opportunity.Title = dto.Title;
        opportunity.Description = dto.Description;
        opportunity.OpportunityType = dto.OpportunityType;
        opportunity.Location = dto.Location;
        opportunity.QualificationRequired = dto.QualificationRequired;
        opportunity.SalaryRange = dto.SalaryRange;
        opportunity.Deadline = dto.Deadline;
        opportunity.Status = dto.Status;
        opportunity.UpdatedAt = DateTime.UtcNow;
        opportunity.RequiredSkills = dto.SkillIds.Select(skillId => new OpportunitySkill
        {
            OpportunityId = id,
            SkillId = skillId
        }).ToList();

        await _repo.UpdateAsync(opportunity);
        await _repo.SaveChangesAsync();

        return MapToDto(opportunity);
    }

    public async Task<bool> DeleteAsync(long id, long employerProfileId)
    {
        var opportunity = await _repo.GetByIdAsync(id);

        if (opportunity == null || opportunity.EmployerProfileId != employerProfileId)
            return false;

        await _repo.DeleteAsync(opportunity);
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task IncrementViewsAsync(long id)
        => await _repo.IncrementViewsAsync(id);

    public async Task<int> GetTotalCountAsync()
        => await _repo.GetTotalCountAsync();

    // ── Manual mapping (no AutoMapper needed) ──
    private static OpportunityDto MapToDto(Opportunity o) => new()
    {
        Id = o.Id,
        Title = o.Title,
        Description = o.Description,
        OpportunityType = o.OpportunityType,
        Location = o.Location,
        QualificationRequired = o.QualificationRequired,
        SalaryRange = o.SalaryRange,
        Status = o.Status,
        Views = o.Views,
        Deadline = o.Deadline,
        CreatedAt = o.CreatedAt,
        EmployerProfileId = o.EmployerProfileId,
        CompanyName = o.EmployerProfile?.CompanyName ?? string.Empty,
        CompanyLogo = o.EmployerProfile?.LogoUrl,
        ApplicationCount = o.Applications?.Count ?? 0,
        RequiredSkills = o.RequiredSkills?
            .Select(rs => rs.Skill?.Name ?? string.Empty)
            .ToList() ?? new()
    };
}