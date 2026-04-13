using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    // GET api/dashboard/admin
    [HttpGet("admin")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetAdminDashboard()
    {
        var dashboard = await _service.GetAdminDashboardAsync();
        return Ok(dashboard);
    }

    // GET api/dashboard/candidate/1
    [HttpGet("candidate/{jobSeekerProfileId}")]
    [Authorize(Roles = "CANDIDATE,ADMIN")]
    public async Task<IActionResult> GetCandidateDashboard(long jobSeekerProfileId)
    {
        var dashboard = await _service.GetCandidateDashboardAsync(jobSeekerProfileId);
        return Ok(dashboard);
    }

    // GET api/dashboard/employer/1
    [HttpGet("employer/{employerProfileId}")]
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    public async Task<IActionResult> GetEmployerDashboard(long employerProfileId)
    {
        var dashboard = await _service.GetEmployerDashboardAsync(employerProfileId);
        return Ok(dashboard);
    }

    // GET api/dashboard/reports/opportunities
    [HttpGet("reports/opportunities")]
    [Authorize(Roles = "ADMIN,EMPLOYER")]
    public async Task<IActionResult> GetOpportunitiesReport(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] string? keyword)
    {
        var filter = new ReportFilterDto
        {
            From = from,
            To = to,
            Status = status,
            Type = type,
            Keyword = keyword
        };
        var report = await _service.GetOpportunitiesReportAsync(filter);
        return Ok(report);
    }

    // GET api/dashboard/reports/applications
    [HttpGet("reports/applications")]
    [Authorize(Roles = "ADMIN,EMPLOYER")]
    public async Task<IActionResult> GetApplicationsReport(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? status)
    {
        var filter = new ReportFilterDto
        {
            From = from,
            To = to,
            Status = status
        };
        var report = await _service.GetApplicationsReportAsync(filter);
        return Ok(report);
    }
}