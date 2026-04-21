using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Services.Interfaces;

public interface IRecommendationService
{
    Task<IEnumerable<MatchedOpportunityDto>> GetRecommendationsForCandidateAsync(long jobSeekerProfileId, int top = 10);
    Task<IEnumerable<MatchedCandidateDto>> GetMatchingCandidatesForOpportunityAsync(long opportunityId, int top = 10);
}
