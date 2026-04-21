using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using JobLinkHub.Data;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "JobSeeker")]
    public class ApplyModel : PageModel
    {
        private readonly AppDbContext _db;
        public ApplyModel(AppDbContext db) { _db = db; }

        [BindProperty] public string? CoverNote { get; set; }
        [BindProperty] public int OpportunityId { get; set; }

        public string JobTitle { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string ApplicantName { get; set; } = "";
        public string ApplicantEmail { get; set; } = "";
        public string? Message { get; set; }
        public bool Success { get; set; }
        public string? NotFound { get; set; }

        public void OnGet(int id)
        {
            OpportunityId = id;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            using var conn = _db.Database.GetDbConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT o.Title, ep.CompanyName FROM Opportunities o
                INNER JOIN EmployerProfiles ep ON o.EmployerProfileId = ep.Id
                WHERE o.Id = @Id AND o.IsActive = 1";
            var p = cmd.CreateParameter(); p.ParameterName = "@Id"; p.Value = id; cmd.Parameters.Add(p);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) { NotFound = "Opportunity not found."; return; }
            JobTitle = r["Title"].ToString()!;
            CompanyName = r["CompanyName"].ToString()!;
            r.Close();

            using var cmd2 = conn.CreateCommand();
            cmd2.CommandText = "SELECT FirstName, LastName, Email FROM AspNetUsers WHERE Id = @UserId";
            var p2 = cmd2.CreateParameter(); p2.ParameterName = "@UserId"; p2.Value = userId ?? (object)DBNull.Value; cmd2.Parameters.Add(p2);
            using var r2 = cmd2.ExecuteReader();
            if (r2.Read())
            {
                ApplicantName = $"{r2["FirstName"]} {r2["LastName"]}".Trim();
                ApplicantEmail = r2["Email"]?.ToString() ?? "";
            }
        }

        public IActionResult OnPost()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            try
            {
                using var conn = _db.Database.GetDbConnection();
                conn.Open();

                // Get seeker profile ID
                using var cmd1 = conn.CreateCommand();
                cmd1.CommandText = "SELECT Id FROM JobSeekerProfiles WHERE UserId = @UserId";
                var pp = cmd1.CreateParameter(); pp.ParameterName = "@UserId"; pp.Value = userId ?? (object)DBNull.Value; cmd1.Parameters.Add(pp);
                var profileIdObj = cmd1.ExecuteScalar();
                if (profileIdObj == null) { Message = "Please complete your profile before applying."; return Page(); }
                int profileId = Convert.ToInt32(profileIdObj);

                // Check already applied
                using var cmdCheck = conn.CreateCommand();
                cmdCheck.CommandText = "SELECT COUNT(*) FROM Applications WHERE JobSeekerProfileId=@PId AND OpportunityId=@OId";
                var cp1 = cmdCheck.CreateParameter(); cp1.ParameterName = "@PId"; cp1.Value = profileId; cmdCheck.Parameters.Add(cp1);
                var cp2 = cmdCheck.CreateParameter(); cp2.ParameterName = "@OId"; cp2.Value = OpportunityId; cmdCheck.Parameters.Add(cp2);
                if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0)
                {
                    Message = "You have already applied for this opportunity.";
                    return Page();
                }

                // Insert application
                using var cmd2 = conn.CreateCommand();
                cmd2.CommandText = @"
                    INSERT INTO Applications (JobSeekerProfileId, OpportunityId, CoverNote, Status, AppliedAt)
                    VALUES (@PId, @OId, @Cover, 'Pending', GETDATE())";
                var ip1 = cmd2.CreateParameter(); ip1.ParameterName = "@PId"; ip1.Value = profileId; cmd2.Parameters.Add(ip1);
                var ip2 = cmd2.CreateParameter(); ip2.ParameterName = "@OId"; ip2.Value = OpportunityId; cmd2.Parameters.Add(ip2);
                var ip3 = cmd2.CreateParameter(); ip3.ParameterName = "@Cover"; ip3.Value = CoverNote ?? ""; cmd2.Parameters.Add(ip3);
                cmd2.ExecuteNonQuery();

                Success = true;
                Message = "Application submitted successfully!";
            }
            catch (Exception ex) { Message = "Error: " + ex.Message; }

            // Reload job info for display
            using var conn2 = _db.Database.GetDbConnection();
            conn2.Open();
            using var cmd3 = conn2.CreateCommand();
            cmd3.CommandText = @"
                SELECT o.Title, ep.CompanyName FROM Opportunities o
                INNER JOIN EmployerProfiles ep ON o.EmployerProfileId = ep.Id
                WHERE o.Id = @Id";
            var rp = cmd3.CreateParameter(); rp.ParameterName = "@Id"; rp.Value = OpportunityId; cmd3.Parameters.Add(rp);
            using var r3 = cmd3.ExecuteReader();
            if (r3.Read()) { JobTitle = r3["Title"].ToString()!; CompanyName = r3["CompanyName"].ToString()!; }

            return Page();
        }
    }
}