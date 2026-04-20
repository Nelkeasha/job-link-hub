using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using JobLinkHub.Data;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "JobSeeker")]
    public class BuildProfileModel : PageModel
    {
        private readonly AppDbContext _db;
        public BuildProfileModel(AppDbContext db) { _db = db; }

        [BindProperty] public string? FirstName { get; set; }
        [BindProperty] public string? LastName { get; set; }
        [BindProperty] public string? Bio { get; set; }
        [BindProperty] public string? Location { get; set; }
        [BindProperty] public string? Phone { get; set; }
        [BindProperty] public string? LinkedInUrl { get; set; }
        [BindProperty] public string? PortfolioUrl { get; set; }

        public string? Message { get; set; }
        public bool Success { get; set; }

        public void OnGet()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            using var conn = _db.Database.GetDbConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT u.FirstName, u.LastName, p.Bio, p.Location, p.Phone, p.LinkedInUrl, p.PortfolioUrl
                FROM AspNetUsers u
                LEFT JOIN JobSeekerProfiles p ON p.UserId = u.Id
                WHERE u.Id = @UserId";
            var p = cmd.CreateParameter(); p.ParameterName = "@UserId"; p.Value = userId ?? (object)DBNull.Value; cmd.Parameters.Add(p);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                FirstName = reader["FirstName"]?.ToString();
                LastName = reader["LastName"]?.ToString();
                Bio = reader["Bio"]?.ToString();
                Location = reader["Location"]?.ToString();
                Phone = reader["Phone"]?.ToString();
                LinkedInUrl = reader["LinkedInUrl"]?.ToString();
                PortfolioUrl = reader["PortfolioUrl"]?.ToString();
            }
        }

        public IActionResult OnPost()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            try
            {
                using var conn = _db.Database.GetDbConnection();
                conn.Open();

                // Update user name
                using var cmd1 = conn.CreateCommand();
                cmd1.CommandText = "UPDATE AspNetUsers SET FirstName=@First, LastName=@Last WHERE Id=@UserId";
                var pp1 = cmd1.CreateParameter(); pp1.ParameterName = "@First"; pp1.Value = FirstName ?? ""; cmd1.Parameters.Add(pp1);
                var pp2 = cmd1.CreateParameter(); pp2.ParameterName = "@Last"; pp2.Value = LastName ?? ""; cmd1.Parameters.Add(pp2);
                var pp3 = cmd1.CreateParameter(); pp3.ParameterName = "@UserId"; pp3.Value = userId ?? (object)DBNull.Value; cmd1.Parameters.Add(pp3);
                cmd1.ExecuteNonQuery();

                // Upsert profile
                using var cmd2 = conn.CreateCommand();
                cmd2.CommandText = @"
                    IF EXISTS (SELECT 1 FROM JobSeekerProfiles WHERE UserId = @UserId)
                        UPDATE JobSeekerProfiles
                        SET Bio=@Bio, Location=@Location, Phone=@Phone,
                            LinkedInUrl=@LinkedIn, PortfolioUrl=@Portfolio
                        WHERE UserId=@UserId
                    ELSE
                        INSERT INTO JobSeekerProfiles (UserId, Bio, Location, Phone, LinkedInUrl, PortfolioUrl)
                        VALUES (@UserId, @Bio, @Location, @Phone, @LinkedIn, @Portfolio)";
                var q1 = cmd2.CreateParameter(); q1.ParameterName = "@UserId"; q1.Value = userId ?? (object)DBNull.Value; cmd2.Parameters.Add(q1);
                var q2 = cmd2.CreateParameter(); q2.ParameterName = "@Bio"; q2.Value = Bio ?? ""; cmd2.Parameters.Add(q2);
                var q3 = cmd2.CreateParameter(); q3.ParameterName = "@Location"; q3.Value = Location ?? ""; cmd2.Parameters.Add(q3);
                var q4 = cmd2.CreateParameter(); q4.ParameterName = "@Phone"; q4.Value = Phone ?? ""; cmd2.Parameters.Add(q4);
                var q5 = cmd2.CreateParameter(); q5.ParameterName = "@LinkedIn"; q5.Value = LinkedInUrl ?? ""; cmd2.Parameters.Add(q5);
                var q6 = cmd2.CreateParameter(); q6.ParameterName = "@Portfolio"; q6.Value = PortfolioUrl ?? ""; cmd2.Parameters.Add(q6);
                cmd2.ExecuteNonQuery();

                Success = true;
                Message = "Profile saved successfully!";
            }
            catch (Exception ex)
            {
                Message = "Error: " + ex.Message;
            }
            return Page();
        }
    }
}