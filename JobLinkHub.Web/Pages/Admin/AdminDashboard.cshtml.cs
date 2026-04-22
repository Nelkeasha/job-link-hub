using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using JobLinkHub.Data;

namespace JobLinkHub.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardModel : PageModel
    {
        private readonly AppDbContext _db;
        public AdminDashboardModel(AppDbContext db) { _db = db; }

        public int TotalUsers { get; set; }
        public int TotalJobSeekers { get; set; }
        public int TotalEmployers { get; set; }
        public int TotalOpportunities { get; set; }
        public int TotalApplications { get; set; }
        public int NewUsersThisMonth { get; set; }

        public void OnGet()
        {
            using var conn = _db.Database.GetDbConnection();
            conn.Open();

            using var cmd1 = conn.CreateCommand(); cmd1.CommandText = "SELECT COUNT(*) FROM AspNetUsers"; TotalUsers = Convert.ToInt32(cmd1.ExecuteScalar());
            using var cmd2 = conn.CreateCommand(); cmd2.CommandText = "SELECT COUNT(*) FROM JobSeekerProfiles"; TotalJobSeekers = Convert.ToInt32(cmd2.ExecuteScalar());
            using var cmd3 = conn.CreateCommand(); cmd3.CommandText = "SELECT COUNT(*) FROM EmployerProfiles"; TotalEmployers = Convert.ToInt32(cmd3.ExecuteScalar());
            using var cmd4 = conn.CreateCommand(); cmd4.CommandText = "SELECT COUNT(*) FROM Opportunities WHERE IsActive=1"; TotalOpportunities = Convert.ToInt32(cmd4.ExecuteScalar());
            using var cmd5 = conn.CreateCommand(); cmd5.CommandText = "SELECT COUNT(*) FROM Applications"; TotalApplications = Convert.ToInt32(cmd5.ExecuteScalar());

            using var cmdMonth = conn.CreateCommand();
            cmdMonth.CommandText = @"
                SELECT COUNT(*) FROM AspNetUsers
                WHERE MONTH(CreatedAt)=MONTH(GETDATE()) AND YEAR(CreatedAt)=YEAR(GETDATE())";
            NewUsersThisMonth = Convert.ToInt32(cmdMonth.ExecuteScalar());
        }
    }
}