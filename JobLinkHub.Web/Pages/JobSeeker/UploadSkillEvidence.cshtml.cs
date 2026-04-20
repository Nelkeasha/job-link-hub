using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using JobLinkHub.Data;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "JobSeeker")]
    public class UploadSkillEvidenceModel : PageModel
    {
        private readonly AppDbContext _db;
        public UploadSkillEvidenceModel(AppDbContext db) { _db = db; }

        [BindProperty] public int SkillId { get; set; }
        [BindProperty] public string? ProficiencyLevel { get; set; }
        [BindProperty] public string? EvidenceType { get; set; }
        [BindProperty] public string? EvidenceUrl { get; set; }
        [BindProperty] public string? Description { get; set; }

        public string? Message { get; set; }
        public bool Success { get; set; }
        public List<SkillOption> AvailableSkills { get; set; } = new();
        public List<EvidenceItem> MyEvidence { get; set; } = new();

        public class SkillOption { public int Id { get; set; } public string Name { get; set; } = ""; }
        public class EvidenceItem
        {
            public int Id { get; set; }
            public string SkillName { get; set; } = "";
            public string EvidenceType { get; set; } = "";
            public string? Url { get; set; }
            public string? Description { get; set; }
            public string ProficiencyLevel { get; set; } = "";
        }

        private string? GetUserId() =>
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        private int GetProfileId(DbConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id FROM JobSeekerProfiles WHERE UserId=@UserId";
            var p = cmd.CreateParameter();
            p.ParameterName = "@UserId";
            p.Value = GetUserId() ?? (object)DBNull.Value;
            cmd.Parameters.Add(p);
            var result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public void OnGet()
        {
            LoadData();
        }

        public IActionResult OnPost()
        {
            try
            {
                using var conn = _db.Database.GetDbConnection();
                conn.Open();
                int profileId = GetProfileId(conn);
                if (profileId == 0) { Message = "Profile not found. Please build your profile first."; LoadData(); return Page(); }

                // Upsert JobSeekerSkill
                using var cmdSkill = conn.CreateCommand();
                cmdSkill.CommandText = @"
                    IF NOT EXISTS (SELECT 1 FROM JobSeekerSkills WHERE JobSeekerProfileId=@ProfileId AND SkillId=@SkillId)
                        INSERT INTO JobSeekerSkills (JobSeekerProfileId, SkillId, ProficiencyLevel)
                        VALUES (@ProfileId, @SkillId, @Level)
                    ELSE
                        UPDATE JobSeekerSkills SET ProficiencyLevel=@Level
                        WHERE JobSeekerProfileId=@ProfileId AND SkillId=@SkillId
                    SELECT Id FROM JobSeekerSkills WHERE JobSeekerProfileId=@ProfileId AND SkillId=@SkillId";
                var p1 = cmdSkill.CreateParameter(); p1.ParameterName = "@ProfileId"; p1.Value = profileId; cmdSkill.Parameters.Add(p1);
                var p2 = cmdSkill.CreateParameter(); p2.ParameterName = "@SkillId"; p2.Value = SkillId; cmdSkill.Parameters.Add(p2);
                var p3 = cmdSkill.CreateParameter(); p3.ParameterName = "@Level"; p3.Value = ProficiencyLevel ?? "Beginner"; cmdSkill.Parameters.Add(p3);
                int jssId = Convert.ToInt32(cmdSkill.ExecuteScalar());

                // Insert evidence
                using var cmdEv = conn.CreateCommand();
                cmdEv.CommandText = @"
                    INSERT INTO SkillEvidences (JobSeekerSkillId, EvidenceType, Url, Description)
                    VALUES (@JssId, @Type, @Url, @Desc)";
                var pe1 = cmdEv.CreateParameter(); pe1.ParameterName = "@JssId"; pe1.Value = jssId; cmdEv.Parameters.Add(pe1);
                var pe2 = cmdEv.CreateParameter(); pe2.ParameterName = "@Type"; pe2.Value = EvidenceType ?? ""; cmdEv.Parameters.Add(pe2);
                var pe3 = cmdEv.CreateParameter(); pe3.ParameterName = "@Url"; pe3.Value = EvidenceUrl ?? ""; cmdEv.Parameters.Add(pe3);
                var pe4 = cmdEv.CreateParameter(); pe4.ParameterName = "@Desc"; pe4.Value = Description ?? ""; cmdEv.Parameters.Add(pe4);
                cmdEv.ExecuteNonQuery();

                Success = true;
                Message = "Evidence added successfully!";
            }
            catch (Exception ex) { Message = "Error: " + ex.Message; }

            LoadData();
            return Page();
        }

        private void LoadData()
        {
            using var conn = _db.Database.GetDbConnection();
            conn.Open();

            // Load all platform skills
            using var cmd1 = conn.CreateCommand();
            cmd1.CommandText = "SELECT Id, Name FROM Skills ORDER BY Name";
            using var r1 = cmd1.ExecuteReader();
            while (r1.Read())
                AvailableSkills.Add(new SkillOption { Id = Convert.ToInt32(r1["Id"]), Name = r1["Name"].ToString()! });
            r1.Close();

            // Load my evidence
            int profileId = GetProfileId(conn);
            if (profileId == 0) return;

            using var cmd2 = conn.CreateCommand();
            cmd2.CommandText = @"
                SELECT se.Id, sk.Name AS SkillName, se.EvidenceType, se.Url, se.Description, jss.ProficiencyLevel
                FROM SkillEvidences se
                INNER JOIN JobSeekerSkills jss ON se.JobSeekerSkillId = jss.Id
                INNER JOIN Skills sk ON jss.SkillId = sk.Id
                WHERE jss.JobSeekerProfileId = @ProfileId
                ORDER BY se.Id DESC";
            var p = cmd2.CreateParameter(); p.ParameterName = "@ProfileId"; p.Value = profileId; cmd2.Parameters.Add(p);
            using var r2 = cmd2.ExecuteReader();
            while (r2.Read())
                MyEvidence.Add(new EvidenceItem
                {
                    Id = Convert.ToInt32(r2["Id"]),
                    SkillName = r2["SkillName"].ToString()!,
                    EvidenceType = r2["EvidenceType"].ToString()!,
                    Url = r2["Url"]?.ToString(),
                    Description = r2["Description"]?.ToString(),
                    ProficiencyLevel = r2["ProficiencyLevel"].ToString()!
                });
        }
    }
}