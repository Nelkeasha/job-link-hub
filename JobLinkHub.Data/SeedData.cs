using JobLinkHub.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace JobLinkHub.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<long>>>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var db = services.GetRequiredService<AppDbContext>();

        // Create roles
        foreach (var role in new[] { "CANDIDATE", "EMPLOYER", "ADMIN" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<long>(role));

        // Create default admin
        const string adminEmail = "admin@joblinkhub.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "JobLinkHub",
                Role = "ADMIN",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin@12345");
            await userManager.AddToRoleAsync(admin, "ADMIN");
        }

        // Seed skills
        if (!db.Skills.Any())
        {
            db.Skills.AddRange(
                new Skill { Name = "C#", Category = "Backend" },
                new Skill { Name = "ASP.NET Core", Category = "Backend" },
                new Skill { Name = "SQL Server", Category = "Database" },
                new Skill { Name = "React", Category = "Frontend" },
                new Skill { Name = "TailwindCSS", Category = "Frontend" },
                new Skill { Name = "JavaScript", Category = "Frontend" },
                new Skill { Name = "Python", Category = "Backend" },
                new Skill { Name = "Docker", Category = "DevOps" },
                new Skill { Name = "Git", Category = "DevOps" },
                new Skill { Name = "UI/UX Design", Category = "Design" }
            );
            await db.SaveChangesAsync();
        }
    }
}