using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using JobLinkHub.Data;

namespace JobLinkHub.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ModerateOpportunitiesModel : PageModel
    {
        private readonly AppDbContext _db;
        public ModerateOpportunitiesModel(AppDbContext db) { _db = db; }

        [BindProperty(SupportsGet = true)] public string? SearchQ { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterType { get; set; }

        public List<OppItem> Opportunities { get; set; } = new();
        public string? Message { get; set; }

        public class OppItem
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public string CompanyName { get; set; } = "";
            public string Type { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public int ApplicationCount { get; set; }
            public bool IsActive { get; set; }
        }

        public void OnGet()
        {
            LoadData();
        }

        public IActionResult OnPostDelete(int id)
        {
            try
            {
                using var conn = _db.Database.GetDbConnection();
                conn.Open();
                // Remove applications first (FK constraint)
                using var cmd1 = conn.CreateCommand();
                cmd1.CommandText = $"DELETE FROM Applications WHERE OpportunityId={id}";
                cmd1.ExecuteNonQuery();

                using var cmd2 = conn.CreateCommand();
                cmd2.CommandText = $"DELETE FROM SavedJobs WHERE OpportunityId={id}";
                cmd2.ExecuteNonQuery();

                using var cmd3 = conn.CreateCommand();
                cmd3.CommandText = $"DELETE FROM OpportunitySkills WHERE OpportunityId={id}";
                cmd3.ExecuteNonQuery();

                using var cmd4 = conn.CreateCommand();
                cmd4.CommandText = $"DELETE FROM Opportunities WHERE Id={id}";
                cmd4.ExecuteNonQuery();
                Message = "Opportunity deleted successfully.";
            }
            catch (Exception ex) { Message = "Error: " + ex.Message; }
            LoadData();
            return Page();
        }

        private void LoadData()
        {
            using var conn = _db.Database.GetDbConnection();
            conn.Open();

            var query = @"
                SELECT o.Id, o.Title, ep.CompanyName, o.Type, o.CreatedAt, o.IsActive,
                       (SELECT COUNT(*) FROM Applications WHERE OpportunityId = o.Id) AS AppCount
                FROM Opportunities o
                INNER JOIN EmployerProfiles ep ON o.EmployerProfileId = ep.Id
                WHERE (@Search IS NULL OR o.Title LIKE '%'+@Search+'%' OR ep.CompanyName LIKE '%'+@Search+'%')
                AND (@Type IS NULL OR o.Type = @Type)
                ORDER BY o.CreatedAt DESC";

            using var cmd = conn.CreateCommand();
            cmd.CommandText = query;

            var pSearch = cmd.CreateParameter();
            pSearch.ParameterName = "@Search";
            pSearch.Value = string.IsNullOrEmpty(SearchQ) ? (object)DBNull.Value : SearchQ;
            cmd.Parameters.Add(pSearch);

            var pType = cmd.CreateParameter();
            pType.ParameterName = "@Type";
            pType.Value = string.IsNullOrEmpty(FilterType) ? (object)DBNull.Value : FilterType;
            cmd.Parameters.Add(pType);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Opportunities.Add(new OppItem
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Title = reader["Title"].ToString()!,
                    CompanyName = reader["CompanyName"].ToString()!,
                    Type = reader["Type"].ToString()!,
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                    ApplicationCount = Convert.ToInt32(reader["AppCount"]),
                    IsActive = Convert.ToBoolean(reader["IsActive"])
                });
            }
        }
    }
}