using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using JobLinkHub.Data;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "JobSeeker")]
    public class MyApplicationsModel : PageModel
    {
        private readonly AppDbContext _db;
        public MyApplicationsModel(AppDbContext db) { _db = db; }

        [BindProperty(SupportsGet = true)] public string? FilterStatus { get; set; }
        public List<AppItem> Applications { get; set; } = new();

        public class AppItem
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Title { get; set; } = "";
            public string CompanyName { get; set; } = "";
            public string Status { get; set; } = "";
            public DateTime AppliedAt { get; set; }
            public string? CoverNote { get; set; }
        }

        public void OnGet()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            using var conn = _db.Database.GetDbConnection();
            conn.Open();

            var query = @"
                SELECT a.Id, a.OpportunityId, o.Title, ep.CompanyName, a.Status, a.AppliedAt, a.CoverNote
                FROM Applications a
                INNER JOIN JobSeekerProfiles p ON a.JobSeekerProfileId = p.Id
                INNER JOIN Opportunities o ON a.OpportunityId = o.Id
                INNER JOIN EmployerProfiles ep ON o.EmployerProfileId = ep.Id
                WHERE p.UserId = @UserId
                AND (@Status IS NULL OR a.Status = @Status)
                ORDER BY a.AppliedAt DESC";

            using var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            var p1 = cmd.CreateParameter(); p1.ParameterName = "@UserId"; p1.Value = userId ?? (object)DBNull.Value; cmd.Parameters.Add(p1);
            var p2 = cmd.CreateParameter(); p2.ParameterName = "@Status"; p2.Value = string.IsNullOrEmpty(FilterStatus) ? (object)DBNull.Value : FilterStatus; cmd.Parameters.Add(p2);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Applications.Add(new AppItem
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    OpportunityId = Convert.ToInt32(reader["OpportunityId"]),
                    Title = reader["Title"].ToString()!,
                    CompanyName = reader["CompanyName"].ToString()!,
                    Status = reader["Status"].ToString()!,
                    AppliedAt = Convert.ToDateTime(reader["AppliedAt"]),
                    CoverNote = reader["CoverNote"]?.ToString()
                });
            }
        }

        public IActionResult OnPostWithdraw(int appId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            using var conn = _db.Database.GetDbConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                DELETE a FROM Applications a
                INNER JOIN JobSeekerProfiles p ON a.JobSeekerProfileId = p.Id
                WHERE a.Id = @AppId AND p.UserId = @UserId";
            var pp = cmd.CreateParameter(); pp.ParameterName = "@AppId"; pp.Value = appId; cmd.Parameters.Add(pp);
            var pp2 = cmd.CreateParameter(); pp2.ParameterName = "@UserId"; pp2.Value = userId ?? (object)DBNull.Value; cmd.Parameters.Add(pp2);
            cmd.ExecuteNonQuery();
            return RedirectToPage();
        }
    }
}