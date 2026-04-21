using FluentAssertions;
using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Implementations;
using Moq;

namespace JobLinkHub.Tests.Services;

public class ApplicationServiceTests
{
    private readonly Mock<IApplicationRepository> _repoMock;
    private readonly ApplicationService _service;

    public ApplicationServiceTests()
    {
        _repoMock = new Mock<IApplicationRepository>();
        _service = new ApplicationService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_NewApplication_ReturnsDto()
    {
        var dto = new CreateApplicationDto
        {
            OpportunityId = 1,
            CoverLetter = "I am a good fit",
            ResumeUsed = "resume.pdf"
        };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Application>()))
            .ReturnsAsync((Application a) => a);
        _repoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<long>()))
            .ReturnsAsync(CreateApplication(1, 1, 10));

        var result = await _service.CreateAsync(10, dto);

        result.Should().NotBeNull();
        result.OpportunityId.Should().Be(1);
        result.Status.Should().Be("PENDING");
    }

    [Fact]
    public async Task UpdateStatusAsync_ValidStatus_ReturnsUpdatedDto()
    {
        var existing = CreateApplication(1, 1, 10);
        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Application>()))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var dto = new UpdateApplicationStatusDto { Status = "SHORTLISTED" };
        var result = await _service.UpdateStatusAsync(1, dto);

        result.Should().NotBeNull();
        result!.Status.Should().Be("SHORTLISTED");
    }

    [Fact]
    public async Task WithdrawAsync_ByOwner_ReturnsTrue()
    {
        var existing = CreateApplication(1, 1, 10);
        _repoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Application>()))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var result = await _service.WithdrawAsync(1, 10);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task WithdrawAsync_ByNonOwner_ReturnsFalse()
    {
        var existing = CreateApplication(1, 1, 10);
        _repoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        var result = await _service.WithdrawAsync(1, 999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByOpportunityAsync_ReturnsApplications()
    {
        var applications = new List<Application>
        {
            CreateApplication(1, 5, 10),
            CreateApplication(2, 5, 11)
        };
        _repoMock.Setup(r => r.GetByOpportunityAsync(5))
            .ReturnsAsync(applications);

        var result = (await _service.GetByOpportunityAsync(5)).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByJobSeekerAsync_ReturnsApplications()
    {
        var applications = new List<Application>
        {
            CreateApplication(1, 5, 10),
            CreateApplication(2, 6, 10)
        };
        _repoMock.Setup(r => r.GetByJobSeekerAsync(10))
            .ReturnsAsync(applications);

        var result = (await _service.GetByJobSeekerAsync(10)).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task HasAppliedAsync_WhenApplied_ReturnsTrue()
    {
        _repoMock.Setup(r => r.HasAppliedAsync(10, 5))
            .ReturnsAsync(true);

        var result = await _service.HasAppliedAsync(10, 5);

        result.Should().BeTrue();
    }

    private static Application CreateApplication(long id, long opportunityId, long jobSeekerProfileId)
    {
        return new Application
        {
            Id = id,
            OpportunityId = opportunityId,
            JobSeekerProfileId = jobSeekerProfileId,
            Status = "PENDING",
            CoverLetter = "Cover letter",
            ResumeUsed = "resume.pdf",
            ApplicationDate = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Opportunity = new Opportunity
            {
                Id = opportunityId,
                Title = "Test Opportunity",
                Description = "Desc",
                EmployerProfile = new EmployerProfile
                {
                    Id = 1,
                    CompanyName = "Test Co",
                    UserId = 1
                }
            },
            JobSeekerProfile = new JobSeekerProfile
            {
                Id = jobSeekerProfileId,
                UserId = 100 + jobSeekerProfileId,
                User = new User
                {
                    Id = 100 + jobSeekerProfileId,
                    FirstName = "Test",
                    LastName = "User",
                    Email = $"test{jobSeekerProfileId}@test.com",
                    UserName = $"test{jobSeekerProfileId}@test.com"
                }
            }
        };
    }
}
