using FluentAssertions;
using JobLinkHub.Data.Entities;
using JobLinkHub.Services.Implementations;
using JobLinkHub.Tests.Helpers;

namespace JobLinkHub.Tests.Services;

public class RecommendationServiceTests
{
    [Fact]
    public async Task GetRecommendationsForCandidateAsync_MatchesSkills()
    {
        var context = TestDbContextFactory.Create();
        await SeedTestData(context);

        var service = new RecommendationService(context);

        var results = (await service.GetRecommendationsForCandidateAsync(1, 10)).ToList();

        results.Should().NotBeEmpty();
        results[0].MatchedSkills.Should().NotBeEmpty();
        results[0].MatchScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetRecommendationsForCandidateAsync_SortsByScore()
    {
        var context = TestDbContextFactory.Create();
        await SeedTestData(context);

        var service = new RecommendationService(context);

        var results = (await service.GetRecommendationsForCandidateAsync(1, 10)).ToList();

        if (results.Count > 1)
        {
            results.Should().BeInDescendingOrder(r => r.MatchScore);
        }
    }

    [Fact]
    public async Task GetRecommendationsForCandidateAsync_NoSkills_ReturnsEmpty()
    {
        var context = TestDbContextFactory.Create();

        // Candidate with no skills
        var user = new User
        {
            Id = 100,
            Email = "noskills@test.com",
            UserName = "noskills@test.com",
            FirstName = "No",
            LastName = "Skills",
            Role = "CANDIDATE",
            IsActive = true
        };
        context.Users.Add(user);

        var profile = new JobSeekerProfile { Id = 100, UserId = 100 };
        context.JobSeekerProfiles.Add(profile);
        await context.SaveChangesAsync();

        var service = new RecommendationService(context);

        var results = (await service.GetRecommendationsForCandidateAsync(100, 10)).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMatchingCandidatesForOpportunityAsync_ReturnsScoredCandidates()
    {
        var context = TestDbContextFactory.Create();
        await SeedTestData(context);

        var service = new RecommendationService(context);

        var results = (await service.GetMatchingCandidatesForOpportunityAsync(1, 10)).ToList();

        results.Should().NotBeEmpty();
        results[0].MatchScore.Should().BeGreaterThan(0);
        results[0].CandidateName.Should().NotBeNullOrEmpty();
    }

    private static async Task SeedTestData(Data.AppDbContext context)
    {
        // Skills
        var skill1 = new Skill { Id = 1, Name = "C#", Category = "Backend" };
        var skill2 = new Skill { Id = 2, Name = "ASP.NET Core", Category = "Backend" };
        var skill3 = new Skill { Id = 3, Name = "React", Category = "Frontend" };
        context.Skills.AddRange(skill1, skill2, skill3);

        // Candidate user
        var candidateUser = new User
        {
            Id = 1,
            Email = "candidate@test.com",
            UserName = "candidate@test.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "CANDIDATE",
            IsActive = true
        };
        context.Users.Add(candidateUser);

        var candidateProfile = new JobSeekerProfile
        {
            Id = 1,
            UserId = 1,
            Location = "Remote"
        };
        context.JobSeekerProfiles.Add(candidateProfile);

        // Candidate skills
        context.JobSeekerSkills.AddRange(
            new JobSeekerSkill { Id = 1, JobSeekerProfileId = 1, SkillId = 1, ProficiencyLevel = "Advanced" },
            new JobSeekerSkill { Id = 2, JobSeekerProfileId = 1, SkillId = 2, ProficiencyLevel = "Intermediate" }
        );

        // Employer user
        var employerUser = new User
        {
            Id = 2,
            Email = "employer@test.com",
            UserName = "employer@test.com",
            FirstName = "Jane",
            LastName = "Corp",
            Role = "EMPLOYER",
            IsActive = true
        };
        context.Users.Add(employerUser);

        var employerProfile = new EmployerProfile
        {
            Id = 1,
            UserId = 2,
            CompanyName = "Test Corp",
            Location = "Remote"
        };
        context.EmployerProfiles.Add(employerProfile);

        // Opportunity 1 — matches 2/2 of candidate's skills
        var opp1 = new Opportunity
        {
            Id = 1,
            EmployerProfileId = 1,
            Title = ".NET Developer",
            Description = "Looking for C# and ASP.NET Core developer",
            Status = "Active",
            Location = "Remote"
        };
        context.Opportunities.Add(opp1);
        context.OpportunitySkills.AddRange(
            new OpportunitySkill { OpportunityId = 1, SkillId = 1 },
            new OpportunitySkill { OpportunityId = 1, SkillId = 2 }
        );

        // Opportunity 2 — matches 1/2 of candidate's skills
        var opp2 = new Opportunity
        {
            Id = 2,
            EmployerProfileId = 1,
            Title = "Full Stack Developer",
            Description = "C# and React developer",
            Status = "Active",
            Location = "Remote"
        };
        context.Opportunities.Add(opp2);
        context.OpportunitySkills.AddRange(
            new OpportunitySkill { OpportunityId = 2, SkillId = 1 },
            new OpportunitySkill { OpportunityId = 2, SkillId = 3 }
        );

        await context.SaveChangesAsync();
    }
}
