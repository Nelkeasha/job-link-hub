using JobLinkHub.Data;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.Services.Implementations;

public class RecommendationService : IRecommendationService
{
    private readonly AppDbContext _context;

    public RecommendationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MatchedOpportunityDto>> GetRecommendationsForCandidateAsync(
        long jobSeekerProfileId, int top = 10)
    {
        var candidateProfile = await _context.JobSeekerProfiles
            .Include(p => p.Skills)
                .ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(p => p.Id == jobSeekerProfileId);

        if (candidateProfile == null)
            return Enumerable.Empty<MatchedOpportunityDto>();

        var candidateSkillIds = candidateProfile.Skills
            .Select(s => s.SkillId)
            .ToHashSet();

        var candidateSkillMap = candidateProfile.Skills
            .ToDictionary(s => s.SkillId, s => s);

        if (candidateSkillIds.Count == 0)
            return Enumerable.Empty<MatchedOpportunityDto>();

        var activeOpportunities = await _context.Opportunities
            .Include(o => o.RequiredSkills)
                .ThenInclude(os => os.Skill)
            .Include(o => o.EmployerProfile)
            .Include(o => o.Applications)
            .Where(o => o.Status == "Active")
            .Where(o => o.RequiredSkills.Any())
            .ToListAsync();

        var results = new List<MatchedOpportunityDto>();

        foreach (var opportunity in activeOpportunities)
        {
            var requiredSkillIds = opportunity.RequiredSkills
                .Select(rs => rs.SkillId)
                .ToHashSet();

            var matchedSkillIds = candidateSkillIds.Intersect(requiredSkillIds).ToList();

            if (matchedSkillIds.Count == 0) continue;

            double baseScore = (double)matchedSkillIds.Count / requiredSkillIds.Count;

            // Boost by proficiency level
            double proficiencyBoost = 0;
            foreach (var skillId in matchedSkillIds)
            {
                if (candidateSkillMap.TryGetValue(skillId, out var candidateSkill))
                {
                    proficiencyBoost += candidateSkill.ProficiencyLevel?.ToLower() switch
                    {
                        "advanced" => 0.2,
                        "intermediate" => 0.1,
                        _ => 0.0
                    };
                }
            }
            proficiencyBoost /= requiredSkillIds.Count;

            // Location boost
            double locationBoost = 0;
            if (!string.IsNullOrEmpty(candidateProfile.Location) &&
                !string.IsNullOrEmpty(opportunity.Location) &&
                candidateProfile.Location.Equals(opportunity.Location, StringComparison.OrdinalIgnoreCase))
            {
                locationBoost = 0.05;
            }

            var matchScore = Math.Min(1.0, baseScore + proficiencyBoost + locationBoost);

            var matchedSkillNames = opportunity.RequiredSkills
                .Where(rs => matchedSkillIds.Contains(rs.SkillId))
                .Select(rs => rs.Skill.Name)
                .ToList();

            var missingSkillNames = opportunity.RequiredSkills
                .Where(rs => !matchedSkillIds.Contains(rs.SkillId))
                .Select(rs => rs.Skill.Name)
                .ToList();

            results.Add(new MatchedOpportunityDto
            {
                Opportunity = MapToOpportunityDto(opportunity),
                MatchScore = Math.Round(matchScore, 3),
                MatchedSkillCount = matchedSkillIds.Count,
                TotalRequiredSkills = requiredSkillIds.Count,
                MatchedSkills = matchedSkillNames,
                MissingSkills = missingSkillNames
            });
        }

        return results
            .OrderByDescending(r => r.MatchScore)
            .Take(top);
    }

    public async Task<IEnumerable<MatchedCandidateDto>> GetMatchingCandidatesForOpportunityAsync(
        long opportunityId, int top = 10)
    {
        var opportunity = await _context.Opportunities
            .Include(o => o.RequiredSkills)
                .ThenInclude(os => os.Skill)
            .FirstOrDefaultAsync(o => o.Id == opportunityId);

        if (opportunity == null)
            return Enumerable.Empty<MatchedCandidateDto>();

        var requiredSkillIds = opportunity.RequiredSkills
            .Select(rs => rs.SkillId)
            .ToHashSet();

        if (requiredSkillIds.Count == 0)
            return Enumerable.Empty<MatchedCandidateDto>();

        var candidates = await _context.JobSeekerProfiles
            .Include(p => p.User)
            .Include(p => p.Skills)
                .ThenInclude(s => s.Skill)
            .Where(p => p.User.IsActive)
            .Where(p => p.Skills.Any(s => requiredSkillIds.Contains(s.SkillId)))
            .ToListAsync();

        var results = new List<MatchedCandidateDto>();

        foreach (var candidate in candidates)
        {
            var candidateSkillIds = candidate.Skills
                .Select(s => s.SkillId)
                .ToHashSet();

            var matchedSkillIds = requiredSkillIds.Intersect(candidateSkillIds).ToList();

            double baseScore = (double)matchedSkillIds.Count / requiredSkillIds.Count;

            double proficiencyBoost = 0;
            foreach (var skillId in matchedSkillIds)
            {
                var candidateSkill = candidate.Skills.FirstOrDefault(s => s.SkillId == skillId);
                if (candidateSkill != null)
                {
                    proficiencyBoost += candidateSkill.ProficiencyLevel?.ToLower() switch
                    {
                        "advanced" => 0.2,
                        "intermediate" => 0.1,
                        _ => 0.0
                    };
                }
            }
            proficiencyBoost /= requiredSkillIds.Count;

            double locationBoost = 0;
            if (!string.IsNullOrEmpty(candidate.Location) &&
                !string.IsNullOrEmpty(opportunity.Location) &&
                candidate.Location.Equals(opportunity.Location, StringComparison.OrdinalIgnoreCase))
            {
                locationBoost = 0.05;
            }

            var matchScore = Math.Min(1.0, baseScore + proficiencyBoost + locationBoost);

            var matchedSkillNames = opportunity.RequiredSkills
                .Where(rs => matchedSkillIds.Contains(rs.SkillId))
                .Select(rs => rs.Skill.Name)
                .ToList();

            var missingSkillNames = opportunity.RequiredSkills
                .Where(rs => !matchedSkillIds.Contains(rs.SkillId))
                .Select(rs => rs.Skill.Name)
                .ToList();

            results.Add(new MatchedCandidateDto
            {
                JobSeekerProfileId = candidate.Id,
                CandidateName = $"{candidate.User.FirstName} {candidate.User.LastName}",
                Email = candidate.User.Email,
                Location = candidate.Location,
                MatchScore = Math.Round(matchScore, 3),
                MatchedSkillCount = matchedSkillIds.Count,
                TotalRequiredSkills = requiredSkillIds.Count,
                MatchedSkills = matchedSkillNames,
                MissingSkills = missingSkillNames
            });
        }

        return results
            .OrderByDescending(r => r.MatchScore)
            .Take(top);
    }

    private static OpportunityDto MapToOpportunityDto(Data.Entities.Opportunity o) => new()
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
        CompanyName = o.EmployerProfile?.CompanyName ?? string.Empty,
        CompanyLogo = o.EmployerProfile?.LogoUrl,
        EmployerProfileId = o.EmployerProfileId,
        ApplicationCount = o.Applications?.Count ?? 0,
        RequiredSkills = o.RequiredSkills?.Select(rs => rs.Skill.Name).ToList() ?? new()
    };
}
