namespace JobLinkHub.Services.DTOs;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalCandidates { get; set; }
    public int TotalEmployers { get; set; }
    public int TotalOpportunities { get; set; }
    public int ActiveOpportunities { get; set; }
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int NewUsersThisMonth { get; set; }
    public List<MonthlyStatDto> ApplicationsPerMonth { get; set; } = new();
    public List<MonthlyStatDto> OpportunitiesPerMonth { get; set; } = new();
    public List<TypeStatDto> OpportunitiesByType { get; set; } = new();
}

public class CandidateDashboardDto
{
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int ShortlistedApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int SavedJobs { get; set; }
    public List<ApplicationDto> RecentApplications { get; set; } = new();
}

public class EmployerDashboardDto
{
    public int TotalOpportunities { get; set; }
    public int ActiveOpportunities { get; set; }
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int ShortlistedApplications { get; set; }
    public int TotalViews { get; set; }
    public List<OpportunityDto> RecentOpportunities { get; set; } = new();
    public List<ApplicationDto> RecentApplications { get; set; } = new();
}

public class MonthlyStatDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TypeStatDto
{
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ReportFilterDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public string? Keyword { get; set; }
}