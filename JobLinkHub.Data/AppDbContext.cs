using JobLinkHub.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<long>, long>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<JobSeekerProfile> JobSeekerProfiles { get; set; }
    public DbSet<EmployerProfile> EmployerProfiles { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<JobSeekerSkill> JobSeekerSkills { get; set; }
    public DbSet<SkillEvidence> SkillEvidences { get; set; }
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<OpportunitySkill> OpportunitySkills { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<SavedJob> SavedJobs { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Composite PK for junction table
        builder.Entity<OpportunitySkill>()
            .HasKey(os => new { os.OpportunityId, os.SkillId });

        // User → JobSeekerProfile (one-to-one)
        builder.Entity<User>()
            .HasOne(u => u.JobSeekerProfile)
            .WithOne(p => p.User)
            .HasForeignKey<JobSeekerProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User → EmployerProfile (one-to-one)
        builder.Entity<User>()
            .HasOne(u => u.EmployerProfile)
            .WithOne(p => p.User)
            .HasForeignKey<EmployerProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Prevent multiple cascade paths
        builder.Entity<Application>()
            .HasOne(a => a.Opportunity)
            .WithMany(o => o.Applications)
            .HasForeignKey(a => a.OpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Application>()
            .HasOne(a => a.JobSeekerProfile)
            .WithMany(p => p.Applications)
            .HasForeignKey(a => a.JobSeekerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SavedJob>()
            .HasOne(s => s.Opportunity)
            .WithMany(o => o.SavedJobs)
            .HasForeignKey(s => s.OpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        builder.Entity<Opportunity>().HasIndex(o => o.Status);
        builder.Entity<Opportunity>().HasIndex(o => o.OpportunityType);
        builder.Entity<Application>().HasIndex(a => a.Status);
        builder.Entity<JobSeekerSkill>()
            .HasIndex(js => new { js.JobSeekerProfileId, js.SkillId })
            .IsUnique();
    }
}