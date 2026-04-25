using JobLinkHub.Data;
using JobLinkHub.Data.Repositories.Interfaces;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly IOpportunityRepository _opportunityRepo;
    private readonly IApplicationRepository _applicationRepo;

    public DashboardService(
        AppDbContext context,
        IOpportunityRepository opportunityRepo,
        IApplicationRepository applicationRepo)
    {
        _context = context;
        _opportunityRepo = opportunityRepo;
        _applicationRepo = applicationRepo;
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalCandidates = await _context.JobSeekerProfiles.CountAsync();
        var totalEmployers = await _context.EmployerProfiles.CountAsync();

        var opportunities = await _context.Opportunities.ToListAsync();
        var applications = await _context.Applications.ToListAsync();

        // Applications per month (last 6 months)
        var appsPerMonth = applications
            .Where(a => a.ApplicationDate >= DateTime.UtcNow.AddMonths(-6))
            .GroupBy(a => a.ApplicationDate.ToString("MMM yyyy"))
            .Select(g => new MonthlyStatDto { Month = g.Key, Count = g.Count() })
            .ToList();

        // Opportunities per month (last 6 months)
        var oppsPerMonth = opportunities
            .Where(o => o.CreatedAt >= DateTime.UtcNow.AddMonths(-6))
            .GroupBy(o => o.CreatedAt.ToString("MMM yyyy"))
            .Select(g => new MonthlyStatDto { Month = g.Key, Count = g.Count() })
            .ToList();

        // Opportunities by type
        var oppsByType = opportunities
            .GroupBy(o => o.OpportunityType)
            .Select(g => new TypeStatDto { Type = g.Key, Count = g.Count() })
            .ToList();

        var firstOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var newUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= firstOfMonth);

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            TotalCandidates = totalCandidates,
            TotalEmployers = totalEmployers,
            TotalOpportunities = opportunities.Count,
            ActiveOpportunities = opportunities.Count(o => o.Status == "Active"),
            TotalApplications = applications.Count,
            PendingApplications = applications.Count(a => a.Status == "PENDING"),
            AcceptedApplications = applications.Count(a => a.Status == "ACCEPTED"),
            RejectedApplications = applications.Count(a => a.Status == "REJECTED"),
            ApplicationsPerMonth = appsPerMonth,
            OpportunitiesPerMonth = oppsPerMonth,
            OpportunitiesByType = oppsByType,
            NewUsersThisMonth = newUsersThisMonth
        };
    }

    public async Task<CandidateDashboardDto> GetCandidateDashboardAsync(long jobSeekerProfileId)
    {
        var applications = await _context.Applications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o.EmployerProfile)
            .Where(a => a.JobSeekerProfileId == jobSeekerProfileId)
            .OrderByDescending(a => a.ApplicationDate)
            .ToListAsync();

        var savedCount = await _context.SavedJobs
            .CountAsync(s => s.JobSeekerProfileId == jobSeekerProfileId);

        var recentApps = applications.Take(5).Select(a => new ApplicationDto
        {
            Id = a.Id,
            OpportunityId = a.OpportunityId,
            OpportunityTitle = a.Opportunity?.Title ?? string.Empty,
            CompanyName = a.Opportunity?.EmployerProfile?.CompanyName ?? string.Empty,
            Status = a.Status,
            ApplicationDate = a.ApplicationDate,
            UpdatedAt = a.UpdatedAt
        }).ToList();

        return new CandidateDashboardDto
        {
            TotalApplications = applications.Count,
            PendingApplications = applications.Count(a => a.Status == "PENDING"),
            ShortlistedApplications = applications.Count(a => a.Status == "SHORTLISTED"),
            AcceptedApplications = applications.Count(a => a.Status == "ACCEPTED"),
            RejectedApplications = applications.Count(a => a.Status == "REJECTED"),
            SavedJobs = savedCount,
            RecentApplications = recentApps
        };
    }

    public async Task<EmployerDashboardDto> GetEmployerDashboardAsync(long employerProfileId)
    {
        var opportunities = await _context.Opportunities
            .Where(o => o.EmployerProfileId == employerProfileId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var opportunityIds = opportunities.Select(o => o.Id).ToList();

        var applications = await _context.Applications
            .Include(a => a.JobSeekerProfile)
                .ThenInclude(p => p.User)
            .Include(a => a.Opportunity)
            .Where(a => opportunityIds.Contains(a.OpportunityId))
            .OrderByDescending(a => a.ApplicationDate)
            .ToListAsync();

        var recentOpps = opportunities.Take(5).Select(o => new OpportunityDto
        {
            Id = o.Id,
            Title = o.Title,
            Status = o.Status,
            Views = o.Views,
            OpportunityType = o.OpportunityType,
            CreatedAt = o.CreatedAt
        }).ToList();

        var recentApps = applications.Take(5).Select(a => new ApplicationDto
        {
            Id = a.Id,
            OpportunityId = a.OpportunityId,
            OpportunityTitle = a.Opportunity?.Title ?? string.Empty,
            CandidateName = a.JobSeekerProfile?.User != null
                ? $"{a.JobSeekerProfile.User.FirstName} {a.JobSeekerProfile.User.LastName}"
                : string.Empty,
            Status = a.Status,
            ApplicationDate = a.ApplicationDate,
            UpdatedAt = a.UpdatedAt
        }).ToList();

        return new EmployerDashboardDto
        {
            TotalOpportunities = opportunities.Count,
            ActiveOpportunities = opportunities.Count(o => o.Status == "Active"),
            TotalApplications = applications.Count,
            PendingApplications = applications.Count(a => a.Status == "PENDING"),
            ShortlistedApplications = applications.Count(a => a.Status == "SHORTLISTED"),
            TotalViews = opportunities.Sum(o => o.Views),
            RecentOpportunities = recentOpps,
            RecentApplications = recentApps
        };
    }

    public async Task<IEnumerable<OpportunityDto>> GetOpportunitiesReportAsync(
        ReportFilterDto filter)
    {
        var query = _context.Opportunities
            .Include(o => o.EmployerProfile)
            .AsQueryable();

        if (filter.From.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.To.Value);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(o => o.Status == filter.Status);

        if (!string.IsNullOrWhiteSpace(filter.Type))
            query = query.Where(o => o.OpportunityType == filter.Type);

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            query = query.Where(o => o.Title.Contains(filter.Keyword));

        var results = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

        return results.Select(o => new OpportunityDto
        {
            Id = o.Id,
            Title = o.Title,
            OpportunityType = o.OpportunityType,
            Location = o.Location,
            Status = o.Status,
            Views = o.Views,
            Deadline = o.Deadline,
            CreatedAt = o.CreatedAt,
            CompanyName = o.EmployerProfile?.CompanyName ?? string.Empty
        });
    }

    public async Task<IEnumerable<ApplicationDto>> GetApplicationsReportAsync(
        ReportFilterDto filter)
    {
        var query = _context.Applications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o.EmployerProfile)
            .Include(a => a.JobSeekerProfile)
                .ThenInclude(p => p.User)
            .AsQueryable();

        if (filter.From.HasValue)
            query = query.Where(a => a.ApplicationDate >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(a => a.ApplicationDate <= filter.To.Value);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(a => a.Status == filter.Status);

        var results = await query
            .OrderByDescending(a => a.ApplicationDate)
            .ToListAsync();

        return results.Select(a => new ApplicationDto
        {
            Id = a.Id,
            OpportunityId = a.OpportunityId,
            OpportunityTitle = a.Opportunity?.Title ?? string.Empty,
            CompanyName = a.Opportunity?.EmployerProfile?.CompanyName ?? string.Empty,
            CandidateName = a.JobSeekerProfile?.User != null
                ? $"{a.JobSeekerProfile.User.FirstName} {a.JobSeekerProfile.User.LastName}"
                : string.Empty,
            CandidateEmail = a.JobSeekerProfile?.User?.Email ?? string.Empty,
            Status = a.Status,
            ApplicationDate = a.ApplicationDate,
            UpdatedAt = a.UpdatedAt
        });
    }
}