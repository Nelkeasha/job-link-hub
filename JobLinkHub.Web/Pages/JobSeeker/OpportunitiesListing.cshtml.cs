using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using JobLinkHub.Data;

namespace JobLinkHub.Web.Pages.JobSeeker
{
    [Authorize(Roles = "JobSeeker")]
    public class OpportunitiesListingModel : PageModel
    {
        private readonly AppDbContext _db;
        public OpportunitiesListingModel(AppDbContext db) { _db = db; }

        [BindProperty(SupportsGet = true)] public string? SearchQ { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterType { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterLocation { get; set; }

        public List<OpportunityItem> Opportunities { get; set; } = new();

        public class OpportunityItem
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public string CompanyName { get; set; } = "";
            public string Type { get; set; } = "";
            public string? Location { get; set; }
            public string? Description { get; set; }
            public DateTime? Deadline { get; set; }
        }

        public void OnGet()
        {
            using var conn = _db.Database.GetDbConnection();
            conn.Open();

            var query = @"
                SELECT o.Id, o.Title, o.Type, o.Location, o.Description, o.Deadline, ep.CompanyName
                FROM Opportunities o
                INNER JOIN EmployerProfiles ep ON o.EmployerProfileId = ep.Id
                WHERE o.IsActive = 1
                AND (@Search IS NULL OR o.Title LIKE '%'+@Search+'%')
                AND (@Type IS NULL OR o.Type = @Type)
                AND (@Location IS NULL OR o.Location LIKE '%'+@Location+'%')
                ORDER BY o.CreatedAt DESC";

            using var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            var p1 = cmd.CreateParameter(); p1.ParameterName = "@Search"; p1.Value = string.IsNullOrEmpty(SearchQ) ? (object)DBNull.Value : SearchQ; cmd.Parameters.Add(p1);
            var p2 = cmd.CreateParameter(); p2.ParameterName = "@Type"; p2.Value = string.IsNullOrEmpty(FilterType) ? (object)DBNull.Value : FilterType; cmd.Parameters.Add(p2);
            var p3 = cmd.CreateParameter(); p3.ParameterName = "@Location"; p3.Value = string.IsNullOrEmpty(FilterLocation) ? (object)DBNull.Value : FilterLocation; cmd.Parameters.Add(p3);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Opportunities.Add(new OpportunityItem
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Title = reader["Title"].ToString()!,
                    CompanyName = reader["CompanyName"].ToString()!,
                    Type = reader["Type"].ToString()!,
                    Location = reader["Location"]?.ToString(),
                    Description = reader["Description"]?.ToString(),
                    Deadline = reader["Deadline"] == DBNull.Value ? null : (DateTime?)reader["Deadline"]
                });
            }
        }
    }
}