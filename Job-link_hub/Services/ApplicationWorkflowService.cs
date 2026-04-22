using JobLinkHub.Data;
using JobLinkHub.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.API.Services;

public class ApplicationWorkflowService(
    AppDbContext dbContext,
    INotificationService notificationService) : IApplicationWorkflowService
{
    private static readonly HashSet<string> AllowedStatuses = ["PENDING", "REVIEWING", "ACCEPTED", "REJECTED", "WITHDRAWN"];

    public async Task<(bool Success, string Message, Application? Application)> ApplyAsync(long userId, long opportunityId, string? coverLetter, string? resumeUsed)
    {
        var profile = await dbContext.JobSeekerProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        if (profile is null) return (false, "Candidate profile not found.", null);

        var opportunity = await dbContext.Opportunities
            .Include(x => x.EmployerProfile)
            .FirstOrDefaultAsync(x => x.Id == opportunityId && x.Status == "ACTIVE");
        if (opportunity is null) return (false, "Opportunity not found or inactive.", null);

        var alreadyApplied = await dbContext.Applications
            .AnyAsync(x => x.OpportunityId == opportunityId && x.JobSeekerProfileId == profile.Id);
        if (alreadyApplied) return (false, "Already applied to this opportunity.", null);

        var app = new Application
        {
            OpportunityId = opportunityId,
            JobSeekerProfileId = profile.Id,
            CoverLetter = coverLetter,
            ResumeUsed = resumeUsed
        };

        dbContext.Applications.Add(app);
        await dbContext.SaveChangesAsync();

        await notificationService.CreateAsync(
            opportunity.EmployerProfile.UserId,
            $"New application received for '{opportunity.Title}'.",
            "APPLICATION");

        return (true, "Application submitted.", app);
    }

    public async Task<(bool Success, string Message)> WithdrawAsync(long userId, long applicationId)
    {
        var profile = await dbContext.JobSeekerProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        if (profile is null) return (false, "Candidate profile not found.");

        var app = await dbContext.Applications
            .FirstOrDefaultAsync(x => x.Id == applicationId && x.JobSeekerProfileId == profile.Id);
        if (app is null) return (false, "Application not found.");
        if (app.Status is "ACCEPTED" or "REJECTED") return (false, "Finalized applications cannot be withdrawn.");

        app.Status = "WITHDRAWN";
        app.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return (true, "Application withdrawn.");
    }

    public async Task<(bool Success, string Message, Application? Application)> UpdateStatusAsync(long userId, long applicationId, string newStatus, string? rejectionReason)
    {
        newStatus = newStatus.ToUpperInvariant();
        if (!AllowedStatuses.Contains(newStatus))
            return (false, "Invalid status transition.", null);

        var employerProfile = await dbContext.EmployerProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        if (employerProfile is null) return (false, "Employer profile not found.", null);

        var app = await dbContext.Applications
            .Include(x => x.Opportunity)
            .ThenInclude(x => x.EmployerProfile)
            .Include(x => x.JobSeekerProfile)
            .FirstOrDefaultAsync(x => x.Id == applicationId);
        if (app is null) return (false, "Application not found.", null);
        if (app.Opportunity.EmployerProfileId != employerProfile.Id) return (false, "Not allowed.", null);

        app.Status = newStatus;
        app.RejectionReason = newStatus == "REJECTED" ? rejectionReason : null;
        app.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        await notificationService.CreateAsync(
            app.JobSeekerProfile.UserId,
            $"Your application for '{app.Opportunity.Title}' is now {newStatus}.",
            "APPLICATION_STATUS");

        return (true, "Application status updated.", app);
    }
}
