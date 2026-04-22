using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using JobLinkHub.Data;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "JobSeeker")]
    public class SavedJobsModel : PageModel
    {
        private readonly AppDbContext _db;
        public SavedJobsModel(AppDbContext db) { _db = db; }

        public List<SavedItem> SavedJobs { get; set; } = new();
        public string? Message { get; set; }

        public class SavedItem
        {
            public int SavedJobId { get; set; }
            public int OpportunityId { get; set; }
            public string Title { get; set; } = "";
            public string CompanyName { get; set; } = "";
            public string Type { get; set; } = "";
            public string? Location { get; set; }
            public DateTime? Deadline { get; set; }
        }

        private int GetProfileId(DbConnection conn)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id FROM JobSeekerProfiles WHERE UserId=@UserId";
            var p = cmd.CreateParameter(); p.ParameterName = "@UserId"; p.Value = userId ?? (object)DBNull.Value; cmd.Parameters.Add(p);
            var r = cmd.ExecuteScalar();
            return r != null ? Convert.ToInt32(r) : 0;
        }

        public void OnGet()
        {
            LoadSaved();
        }

        public IActionResult OnPostRemove(int savedJobId)
        {
            using var conn = _db.Database.GetDbConnection();
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM SavedJobs WHERE Id=@Id";
            var p = cmd.CreateParameter(); p.ParameterName = "@Id"; p.Value = savedJobId; cmd.Parameters.Add(p);
            cmd.ExecuteNonQuery();
            LoadSaved();
            return Page();
        }

        private void LoadSaved()
        {
            using var conn = _db.Database.GetDbConnection();
            conn.Open();
            int profileId = GetProfileId(conn);
            if (profileId == 0) return;
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT sj.Id AS SavedJobId, o.Id AS OppId, o.Title, ep.CompanyName, o.Type, o.Location, o.Deadline
                FROM SavedJobs sj
                INNER JOIN Opportunities o ON sj.OpportunityId = o.Id
                INNER JOIN EmployerProfiles ep ON o.EmployerProfileId = ep.Id
                WHERE sj.JobSeekerProfileId = @ProfileId
                ORDER BY sj.SavedAt DESC";
            var p = cmd.CreateParameter(); p.ParameterName = "@ProfileId"; p.Value = profileId; cmd.Parameters.Add(p);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                SavedJobs.Add(new SavedItem
                {
                    SavedJobId = Convert.ToInt32(reader["SavedJobId"]),
                    OpportunityId = Convert.ToInt32(reader["OppId"]),
                    Title = reader["Title"].ToString()!,
                    CompanyName = reader["CompanyName"].ToString()!,
                    Type = reader["Type"].ToString()!,
                    Location = reader["Location"]?.ToString(),
                    Deadline = reader["Deadline"] == DBNull.Value ? null : (DateTime?)reader["Deadline"]
                });
            }
        }
    }
}