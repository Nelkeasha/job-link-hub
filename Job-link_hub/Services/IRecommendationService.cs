using JobLinkHub.API.Models.Recommendations;

namespace JobLinkHub.API.Services;

public interface IRecommendationService
{
    Task<IReadOnlyList<RecommendedOpportunityDto>> GetRecommendedOpportunitiesAsync(long userId, int take = 10);
}
