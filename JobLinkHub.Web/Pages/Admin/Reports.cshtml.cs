using ClosedXML.Excel;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JobLinkHub.Web.Pages.Admin
{
    [Authorize(Roles = "ADMIN")]
    public class ReportsModel : PageModel
    {
        private readonly IDashboardService _dashboard;
        private readonly IAdminService _admin;

        public ReportsModel(IDashboardService dashboard, IAdminService admin)
        {
            _dashboard = dashboard;
            _admin = admin;
        }

        [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterRole { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterType { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }

        public AdminDashboardDto Stats { get; set; } = new();
        public List<UserListDto> Users { get; set; } = new();
        public List<AdminOpportunityDto> Opportunities { get; set; } = new();
        public List<ApplicationDto> Applications { get; set; } = new();

        public async Task OnGetAsync()
        {
            Stats = await _dashboard.GetAdminDashboardAsync();

            var allUsers = await _admin.GetAllUsersAsync(FilterRole, null);
            Users = FilterByDate(allUsers, u => u.CreatedAt).ToList();

            var allOpp = await _admin.GetAllOpportunitiesForModerationAsync();
            if (!string.IsNullOrWhiteSpace(FilterType))
                allOpp = allOpp.Where(o => o.OpportunityType == FilterType);
            Opportunities = allOpp.ToList();

            var filter = new ReportFilterDto { From = FromDate, To = ToDate, Status = FilterStatus, Type = FilterType };
            var allApps = await _dashboard.GetApplicationsReportAsync(filter);
            Applications = allApps.ToList();
        }

        public async Task<IActionResult> OnGetExportUsersAsync()
        {
            var users = (await _admin.GetAllUsersAsync(FilterRole, null)).ToList();
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Users");
            ws.Cell(1, 1).Value = "ID"; ws.Cell(1, 2).Value = "First Name"; ws.Cell(1, 3).Value = "Last Name";
            ws.Cell(1, 4).Value = "Email"; ws.Cell(1, 5).Value = "Role"; ws.Cell(1, 6).Value = "Active"; ws.Cell(1, 7).Value = "Registered";
            ws.Row(1).Style.Font.Bold = true;
            for (int i = 0; i < users.Count; i++)
            {
                var u = users[i]; int r = i + 2;
                ws.Cell(r, 1).Value = u.Id; ws.Cell(r, 2).Value = u.FirstName; ws.Cell(r, 3).Value = u.LastName;
                ws.Cell(r, 4).Value = u.Email; ws.Cell(r, 5).Value = u.Role;
                ws.Cell(r, 6).Value = u.IsActive ? "Yes" : "No"; ws.Cell(r, 7).Value = u.CreatedAt.ToString("yyyy-MM-dd");
            }
            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "users_report.xlsx");
        }

        public async Task<IActionResult> OnGetExportOpportunitiesAsync()
        {
            var opps = (await _admin.GetAllOpportunitiesForModerationAsync()).ToList();
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Opportunities");
            ws.Cell(1, 1).Value = "ID"; ws.Cell(1, 2).Value = "Title"; ws.Cell(1, 3).Value = "Company";
            ws.Cell(1, 4).Value = "Type"; ws.Cell(1, 5).Value = "Status"; ws.Cell(1, 6).Value = "Applications"; ws.Cell(1, 7).Value = "Created";
            ws.Row(1).Style.Font.Bold = true;
            for (int i = 0; i < opps.Count; i++)
            {
                var o = opps[i]; int r = i + 2;
                ws.Cell(r, 1).Value = o.Id; ws.Cell(r, 2).Value = o.Title; ws.Cell(r, 3).Value = o.CompanyName;
                ws.Cell(r, 4).Value = o.OpportunityType; ws.Cell(r, 5).Value = o.Status;
                ws.Cell(r, 6).Value = o.ApplicationCount; ws.Cell(r, 7).Value = o.CreatedAt.ToString("yyyy-MM-dd");
            }
            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "opportunities_report.xlsx");
        }

        public async Task<IActionResult> OnGetExportApplicationsAsync()
        {
            var filter = new ReportFilterDto { From = FromDate, To = ToDate, Status = FilterStatus };
            var apps = (await _dashboard.GetApplicationsReportAsync(filter)).ToList();
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Applications");
            ws.Cell(1, 1).Value = "ID"; ws.Cell(1, 2).Value = "Opportunity"; ws.Cell(1, 3).Value = "Company";
            ws.Cell(1, 4).Value = "Candidate"; ws.Cell(1, 5).Value = "Status"; ws.Cell(1, 6).Value = "Applied Date";
            ws.Row(1).Style.Font.Bold = true;
            for (int i = 0; i < apps.Count; i++)
            {
                var a = apps[i]; int r = i + 2;
                ws.Cell(r, 1).Value = a.Id; ws.Cell(r, 2).Value = a.OpportunityTitle; ws.Cell(r, 3).Value = a.CompanyName;
                ws.Cell(r, 4).Value = a.CandidateName; ws.Cell(r, 5).Value = a.Status;
                ws.Cell(r, 6).Value = a.ApplicationDate.ToString("yyyy-MM-dd");
            }
            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "applications_report.xlsx");
        }

        private IEnumerable<T> FilterByDate<T>(IEnumerable<T> items, Func<T, DateTime> dateSelector)
        {
            if (FromDate.HasValue) items = items.Where(i => dateSelector(i) >= FromDate.Value);
            if (ToDate.HasValue) items = items.Where(i => dateSelector(i) <= ToDate.Value);
            return items;
        }
    }
}
