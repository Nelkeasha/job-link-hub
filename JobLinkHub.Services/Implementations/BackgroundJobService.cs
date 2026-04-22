using JobLinkHub.Data;
using JobLinkHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobLinkHub.Services.Implementations;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly IRecommendationService _recommendationService;
    private readonly ILogger<BackgroundJobService> _logger;

    public BackgroundJobService(
        AppDbContext context,
        IEmailService emailService,
        INotificationService notificationService,
        IRecommendationService recommendationService,
        ILogger<BackgroundJobService> logger)
    {
        _context = context;
        _emailService = emailService;
        _notificationService = notificationService;
        _recommendationService = recommendationService;
        _logger = logger;
    }

    public async Task SendDeadlineRemindersAsync()
    {
        _logger.LogInformation("Running deadline reminders job");

        var threeDaysFromNow = DateTime.UtcNow.AddDays(3);
        var now = DateTime.UtcNow;

        var closingOpportunities = await _context.Opportunities
            .Include(o => o.EmployerProfile)
            .Where(o => o.Status == "Active"
                && o.Deadline != null
                && o.Deadline > now
                && o.Deadline <= threeDaysFromNow)
            .ToListAsync();

        if (!closingOpportunities.Any()) return;

        // Get candidates who saved these opportunities
        var opportunityIds = closingOpportunities.Select(o => o.Id).ToList();
        var savedJobs = await _context.SavedJobs
            .Include(sj => sj.JobSeekerProfile)
                .ThenInclude(p => p.User)
            .Where(sj => opportunityIds.Contains(sj.OpportunityId))
            .ToListAsync();

        foreach (var savedJob in savedJobs)
        {
            var user = savedJob.JobSeekerProfile?.User;
            if (user == null) continue;

            var opportunity = closingOpportunities.First(o => o.Id == savedJob.OpportunityId);
            var daysLeft = (opportunity.Deadline!.Value - now).Days;

            await _notificationService.CreateAsync(
                user.Id,
                $"Deadline reminder: '{opportunity.Title}' closes in {daysLeft} day(s)!",
                "DeadlineReminder");
        }

        _logger.LogInformation("Deadline reminders sent for {Count} opportunities", closingOpportunities.Count);
    }

    public async Task SendWeeklyRecommendationsAsync()
    {
        _logger.LogInformation("Running weekly recommendations job");

        var candidates = await _context.JobSeekerProfiles
            .Include(p => p.User)
            .Include(p => p.Skills)
            .Where(p => p.User.IsActive && p.Skills.Any())
            .ToListAsync();

        foreach (var candidate in candidates)
        {
            var recommendations = await _recommendationService
                .GetRecommendationsForCandidateAsync(candidate.Id, 5);

            var topMatches = recommendations.ToList();
            if (!topMatches.Any()) continue;

            var opportunityTitles = topMatches
                .Select(r => r.Opportunity.Title)
                .ToList();

            await _emailService.SendOpportunityMatchAsync(
                candidate.User.Email!,
                $"{candidate.User.FirstName} {candidate.User.LastName}",
                opportunityTitles);

            await _notificationService.CreateAsync(
                candidate.User.Id,
                $"We found {topMatches.Count} new opportunities matching your skills!",
                "WeeklyRecommendation");
        }

        _logger.LogInformation("Weekly recommendations sent to {Count} candidates", candidates.Count);
    }

    public async Task CleanupExpiredTokensAsync()
    {
        _logger.LogInformation("Running token cleanup job");

        var cutoff = DateTime.UtcNow.AddDays(-7);
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < cutoff || (rt.RevokedAt != null && rt.RevokedAt < cutoff))
            .ToListAsync();

        if (expiredTokens.Any())
        {
            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Cleaned up {Count} expired/revoked tokens", expiredTokens.Count);
    }

    public async Task SendApplicationFollowUpAsync()
    {
        _logger.LogInformation("Running application follow-up job");

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var eightDaysAgo = DateTime.UtcNow.AddDays(-8);

        // Find applications that have been pending for exactly ~7 days
        var pendingApplications = await _context.Applications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o.EmployerProfile)
                    .ThenInclude(ep => ep.User)
            .Include(a => a.JobSeekerProfile)
                .ThenInclude(p => p.User)
            .Where(a => a.Status == "PENDING"
                && a.ApplicationDate <= sevenDaysAgo
                && a.ApplicationDate > eightDaysAgo)
            .ToListAsync();

        foreach (var application in pendingApplications)
        {
            var candidateUser = application.JobSeekerProfile?.User;
            if (candidateUser == null) continue;

            await _notificationService.CreateAsync(
                candidateUser.Id,
                $"Your application for '{application.Opportunity?.Title ?? "Unknown"}' has been pending for 7 days. Hang tight!",
                "ApplicationFollowUp");

            // Also notify employer
            var employerUser = application.Opportunity?.EmployerProfile?.User;
            if (employerUser != null)
            {
                await _notificationService.CreateAsync(
                    employerUser.Id,
                    $"Reminder: Application from {candidateUser.FirstName} {candidateUser.LastName} for '{application.Opportunity!.Title}' is pending for 7 days.",
                    "ApplicationFollowUp");
            }
        }

        _logger.LogInformation("Follow-up sent for {Count} pending applications", pendingApplications.Count);
    }
}
