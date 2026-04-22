using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,EMPLOYER")]
public class ExportController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public ExportController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        // Required for EPPlus non-commercial use
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    // GET api/export/opportunities
    [HttpGet("opportunities")]
    public async Task<IActionResult> ExportOpportunities(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? status,
        [FromQuery] string? type)
    {
        var filter = new ReportFilterDto
        {
            From = from,
            To = to,
            Status = status,
            Type = type
        };

        var data = await _dashboardService.GetOpportunitiesReportAsync(filter);

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Opportunities");

        // Headers
        var headers = new[]
        {
            "ID", "Title", "Type", "Location",
            "Status", "Salary Range", "Company",
            "Views", "Deadline", "Created At"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(27, 79, 114));
        }

        // Data
        var row = 2;
        foreach (var o in data)
        {
            worksheet.Cells[row, 1].Value = o.Id;
            worksheet.Cells[row, 2].Value = o.Title;
            worksheet.Cells[row, 3].Value = o.OpportunityType;
            worksheet.Cells[row, 4].Value = o.Location ?? "-";
            worksheet.Cells[row, 5].Value = o.Status;
            worksheet.Cells[row, 6].Value = o.SalaryRange ?? "-";
            worksheet.Cells[row, 7].Value = o.CompanyName;
            worksheet.Cells[row, 8].Value = o.Views;
            worksheet.Cells[row, 9].Value = o.Deadline?.ToString("yyyy-MM-dd") ?? "-";
            worksheet.Cells[row, 10].Value = o.CreatedAt.ToString("yyyy-MM-dd");
            row++;
        }

        worksheet.Cells.AutoFitColumns();

        var stream = new MemoryStream(package.GetAsByteArray());
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Opportunities_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // GET api/export/applications
    [HttpGet("applications")]
    public async Task<IActionResult> ExportApplications(
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

        var data = await _dashboardService.GetApplicationsReportAsync(filter);

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Applications");

        // Headers
        var headers = new[]
        {
            "ID", "Candidate Name", "Candidate Email",
            "Opportunity", "Company", "Status",
            "Applied Date", "Last Updated"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(27, 79, 114));
        }

        // Data
        var row = 2;
        foreach (var a in data)
        {
            worksheet.Cells[row, 1].Value = a.Id;
            worksheet.Cells[row, 2].Value = a.CandidateName;
            worksheet.Cells[row, 3].Value = a.CandidateEmail;
            worksheet.Cells[row, 4].Value = a.OpportunityTitle;
            worksheet.Cells[row, 5].Value = a.CompanyName;
            worksheet.Cells[row, 6].Value = a.Status;
            worksheet.Cells[row, 7].Value = a.ApplicationDate.ToString("yyyy-MM-dd");
            worksheet.Cells[row, 8].Value = a.UpdatedAt.ToString("yyyy-MM-dd");
            row++;
        }

        worksheet.Cells.AutoFitColumns();

        var stream = new MemoryStream(package.GetAsByteArray());
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Applications_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // GET api/export/users
    [HttpGet("users")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> ExportUsers()
    {
        var data = await _dashboardService
            .GetApplicationsReportAsync(new ReportFilterDto());

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Users");

        var headers = new[]
        {
            "Candidate Name", "Email",
            "Opportunity Applied", "Status", "Applied Date"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(Color.White);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(27, 79, 114));
        }

        var row = 2;
        foreach (var a in data)
        {
            worksheet.Cells[row, 1].Value = a.CandidateName;
            worksheet.Cells[row, 2].Value = a.CandidateEmail;
            worksheet.Cells[row, 3].Value = a.OpportunityTitle;
            worksheet.Cells[row, 4].Value = a.Status;
            worksheet.Cells[row, 5].Value = a.ApplicationDate.ToString("yyyy-MM-dd");
            row++;
        }

        worksheet.Cells.AutoFitColumns();

        var stream = new MemoryStream(package.GetAsByteArray());
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Users_{DateTime.Now:yyyyMMdd}.xlsx");
    }
}