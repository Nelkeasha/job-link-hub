namespace JobLinkHub.Services.Interfaces;

public interface IBackgroundJobService
{
    Task SendDeadlineRemindersAsync();
    Task SendWeeklyRecommendationsAsync();
    Task CleanupExpiredTokensAsync();
    Task SendApplicationFollowUpAsync();
}
