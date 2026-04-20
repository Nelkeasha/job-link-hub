using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using JobLinkHub.Data;

namespace JobLinkHub.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ManageUsersModel : PageModel
    {
        private readonly AppDbContext _db;
        public ManageUsersModel(AppDbContext db) { _db = db; }

        [BindProperty(SupportsGet = true)] public string? SearchQ { get; set; }
        [BindProperty(SupportsGet = true)] public string? FilterRole { get; set; }

        public List<UserItem> Users { get; set; } = new();

        public class UserItem
        {
            public string Id { get; set; } = "";
            public string FullName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Role { get; set; } = "";
            public DateTime? CreatedAt { get; set; }
            public bool IsActive { get; set; }
        }

        public void OnGet()
        {
            using var conn = _db.Database.GetDbConnection();
            conn.Open();

            var query = @"
                SELECT u.Id, u.FirstName, u.LastName, u.Email, r.Name AS Role, u.CreatedAt, u.IsActive
                FROM AspNetUsers u
                LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
                WHERE (@Search IS NULL OR u.Email LIKE '%'+@Search+'%'
                                       OR u.FirstName LIKE '%'+@Search+'%'
                                       OR u.LastName LIKE '%'+@Search+'%')
                AND (@Role IS NULL OR r.Name = @Role)
                ORDER BY u.CreatedAt DESC";

            using var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            var pSearch = cmd.CreateParameter(); pSearch.ParameterName = "@Search"; pSearch.Value = string.IsNullOrEmpty(SearchQ) ? (object)DBNull.Value : SearchQ; cmd.Parameters.Add(pSearch);
            var pRole = cmd.CreateParameter(); pRole.ParameterName = "@Role"; pRole.Value = string.IsNullOrEmpty(FilterRole) ? (object)DBNull.Value : FilterRole; cmd.Parameters.Add(pRole);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Users.Add(new UserItem
                {
                    Id = reader["Id"].ToString()!,
                    FullName = $"{reader["FirstName"]} {reader["LastName"]}".Trim(),
                    Email = reader["Email"].ToString()!,
                    Role = reader["Role"]?.ToString() ?? "—",
                    CreatedAt = reader["CreatedAt"] == DBNull.Value ? null : (DateTime?)reader["CreatedAt"],
                    IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"])
                });
            }
        }
    }
}