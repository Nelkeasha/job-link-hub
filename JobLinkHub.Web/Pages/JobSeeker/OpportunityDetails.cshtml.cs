using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using JobLinkHub.Data;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "JobSeeker")]
    public class OpportunityDetailsModel : PageModel
    {
        private readonly AppDbContext _db;
        public OpportunityDetailsModel(AppDbContext db) { _db = db; }

        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string CompanyName { get; set; } = "";
        public string Type { get; set; } = "";
        public string? Location { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public string? EmployerDescription { get; set; }
        public List<string> RequiredSkills { get; set; } = new();
        public bool AlreadyApplied { get; set; }
        public string? NotFound { get; set; }

        public void OnGet(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            using var conn = _db.Database.GetDbConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT o.Id, o.Title, o.Type, o.Location, o.Description, o.Deadline,
                       ep.CompanyName, ep.Description AS EmpDesc
                FROM Opportunities o
                INNER JOIN EmployerProfiles ep ON o.EmployerProfileId = ep.Id
                WHERE o.Id = @Id AND o.IsActive = 1";
            var p = cmd.CreateParameter(); p.ParameterName = "@Id"; p.Value = id; cmd.Parameters.Add(p);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) { NotFound = "Opportunity not found."; return; }

            Id = Convert.ToInt32(reader["Id"]);
            Title = reader["Title"].ToString()!;
            CompanyName = reader["CompanyName"].ToString()!;
            Type = reader["Type"].ToString()!;
            Location = reader["Location"]?.ToString();
            Description = reader["Description"]?.ToString();
            Deadline = reader["Deadline"] == DBNull.Value ? null : (DateTime?)reader["Deadline"];
            EmployerDescription = reader["EmpDesc"]?.ToString();
            reader.Close();

            // Required skills
            using var cmd2 = conn.CreateCommand();
            cmd2.CommandText = @"
                SELECT s.Name FROM OpportunitySkills os
                INNER JOIN Skills s ON os.SkillId = s.Id
                WHERE os.OpportunityId = @Id";
            var p2 = cmd2.CreateParameter(); p2.ParameterName = "@Id"; p2.Value = id; cmd2.Parameters.Add(p2);
            using var r2 = cmd2.ExecuteReader();
            while (r2.Read()) RequiredSkills.Add(r2["Name"].ToString()!);
            r2.Close();

            // Check if already applied
            using var cmd3 = conn.CreateCommand();
            cmd3.CommandText = @"
                SELECT COUNT(*) FROM Applications a
                INNER JOIN JobSeekerProfiles p ON a.JobSeekerProfileId = p.Id
                WHERE p.UserId = @UserId AND a.OpportunityId = @Id";
            var p3 = cmd3.CreateParameter(); p3.ParameterName = "@UserId"; p3.Value = userId ?? (object)DBNull.Value; cmd3.Parameters.Add(p3);
            var p4 = cmd3.CreateParameter(); p4.ParameterName = "@Id"; p4.Value = id; cmd3.Parameters.Add(p4);
            AlreadyApplied = Convert.ToInt32(cmd3.ExecuteScalar()) > 0;
        }
    }
}