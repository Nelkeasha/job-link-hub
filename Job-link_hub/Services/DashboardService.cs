using JobLinkHub.API.Models.Dashboard;
using JobLinkHub.Data;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.API.Services;

public class DashboardService(AppDbContext dbContext) : IDashboardService
{
    public async Task<CandidateDashboardStatsDto> GetCandidateStatsAsync(long userId)
    {
        var profileId = await dbContext.JobSeekerProfiles
            .Where(x => x.UserId == userId)
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync();

        if (profileId is null) return new CandidateDashboardStatsDto();

        var apps = dbContext.Applications.Where(x => x.JobSeekerProfileId == profileId.Value);

        return new CandidateDashboardStatsDto
        {
            TotalApplications = await apps.CountAsync(),
            PendingApplications = await apps.CountAsync(x => x.Status == "PENDING"),
            AcceptedApplications = await apps.CountAsync(x => x.Status == "ACCEPTED"),
            RejectedApplications = await apps.CountAsync(x => x.Status == "REJECTED"),
            SavedJobs = await dbContext.SavedJobs.CountAsync(x => x.JobSeekerProfileId == profileId.Value)
        };
    }

    public async Task<EmployerDashboardStatsDto> GetEmployerStatsAsync(long userId)
    {
        var employerProfileId = await dbContext.EmployerProfiles
            .Where(x => x.UserId == userId)
            .Select(x => (long?)x.Id)
            .FirstOrDefaultAsync();

        if (employerProfileId is null) return new EmployerDashboardStatsDto();

        var opportunities = dbContext.Opportunities.Where(x => x.EmployerProfileId == employerProfileId.Value);
        var opportunityIds = opportunities.Select(x => x.Id);

        return new EmployerDashboardStatsDto
        {
            TotalOpportunities = await opportunities.CountAsync(),
            ActiveOpportunities = await opportunities.CountAsync(x => x.Status == "ACTIVE"),
            ClosedOpportunities = await opportunities.CountAsync(x => x.Status == "CLOSED"),
            TotalApplicationsReceived = await dbContext.Applications.CountAsync(x => opportunityIds.Contains(x.OpportunityId))
        };
    }

    public async Task<AdminDashboardStatsDto> GetAdminStatsAsync()
    {
        return new AdminDashboardStatsDto
        {
            TotalUsers = await dbContext.Users.CountAsync(),
            TotalCandidates = await dbContext.Users.CountAsync(x => x.Role == "CANDIDATE"),
            TotalEmployers = await dbContext.Users.CountAsync(x => x.Role == "EMPLOYER"),
            TotalAdmins = await dbContext.Users.CountAsync(x => x.Role == "ADMIN"),
            TotalOpportunities = await dbContext.Opportunities.CountAsync(),
            TotalApplications = await dbContext.Applications.CountAsync()
        };
    }
}
