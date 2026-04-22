namespace JobLinkHub.API.Models.Dashboard;

public class CandidateDashboardStatsDto
{
    public int TotalApplications { get; set; }
    public int PendingApplications { get; set; }
    public int AcceptedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int SavedJobs { get; set; }
}

public class EmployerDashboardStatsDto
{
    public int TotalOpportunities { get; set; }
    public int ActiveOpportunities { get; set; }
    public int ClosedOpportunities { get; set; }
    public int TotalApplicationsReceived { get; set; }
}

public class AdminDashboardStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalCandidates { get; set; }
    public int TotalEmployers { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalOpportunities { get; set; }
    public int TotalApplications { get; set; }
}
