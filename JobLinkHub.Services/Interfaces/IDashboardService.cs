using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Services.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync();
    Task<CandidateDashboardDto> GetCandidateDashboardAsync(long jobSeekerProfileId);
    Task<EmployerDashboardDto> GetEmployerDashboardAsync(long employerProfileId);
    Task<IEnumerable<OpportunityDto>> GetOpportunitiesReportAsync(ReportFilterDto filter);
    Task<IEnumerable<ApplicationDto>> GetApplicationsReportAsync(ReportFilterDto filter);
}