using JobLinkHub.API.Models.Dashboard;

namespace JobLinkHub.API.Services;

public interface IDashboardService
{
    Task<CandidateDashboardStatsDto> GetCandidateStatsAsync(long userId);
    Task<EmployerDashboardStatsDto> GetEmployerStatsAsync(long userId);
    Task<AdminDashboardStatsDto> GetAdminStatsAsync();
}
