using JobLinkHub.API.Models.Recommendations;
using JobLinkHub.Data;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.API.Services;

public class RecommendationService(AppDbContext dbContext) : IRecommendationService
{
    public async Task<IReadOnlyList<RecommendedOpportunityDto>> GetRecommendedOpportunitiesAsync(long userId, int take = 10)
    {
        var profileId = await dbContext.JobSeekerProfiles
            .Where(x => x.UserId == userId)
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync();
        if (profileId is null) return [];

        var skillIds = await dbContext.JobSeekerSkills
            .Where(x => x.JobSeekerProfileId == profileId.Value)
            .Select(x => x.SkillId)
            .ToListAsync();
        if (skillIds.Count == 0) return [];

        var query = dbContext.Opportunities
            .Where(x => x.Status == "ACTIVE")
            .Select(x => new RecommendedOpportunityDto
            {
                OpportunityId = x.Id,
                Title = x.Title,
                OpportunityType = x.OpportunityType,
                Location = x.Location,
                EmployerName = x.EmployerProfile.CompanyName,
                MatchingSkillsCount = x.RequiredSkills.Count(rs => skillIds.Contains(rs.SkillId))
            })
            .Where(x => x.MatchingSkillsCount > 0)
            .OrderByDescending(x => x.MatchingSkillsCount)
            .ThenByDescending(x => x.OpportunityId)
            .Take(take);

        return await query.ToListAsync();
    }
}
