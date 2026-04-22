using FluentAssertions;
using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Implementations;
using Moq;

namespace JobLinkHub.Tests.Services;

public class SavedJobServiceTests
{
    private readonly Mock<ISavedJobRepository> _repoMock;
    private readonly SavedJobService _service;

    public SavedJobServiceTests()
    {
        _repoMock = new Mock<ISavedJobRepository>();
        _service = new SavedJobService(_repoMock.Object);
    }

    [Fact]
    public async Task SaveJobAsync_NewJob_ReturnsDto()
    {
        var dto = new SaveJobDto { OpportunityId = 5, Notes = "Interesting" };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<SavedJob>()))
            .ReturnsAsync((SavedJob s) => s);
        _repoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.GetByJobSeekerAndOpportunityAsync(10, 5))
            .ReturnsAsync(CreateSavedJob(1, 10, 5));

        var result = await _service.SaveJobAsync(10, dto);

        result.Should().NotBeNull();
        result.OpportunityId.Should().Be(5);
    }

    [Fact]
    public async Task UnsaveJobAsync_Existing_ReturnsTrue()
    {
        var existing = CreateSavedJob(1, 10, 5);
        _repoMock.Setup(r => r.GetByJobSeekerAndOpportunityAsync(10, 5))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.DeleteAsync(existing))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var result = await _service.UnsaveJobAsync(10, 5);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task UnsaveJobAsync_NotExisting_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetByJobSeekerAndOpportunityAsync(10, 999))
            .ReturnsAsync((SavedJob?)null);

        var result = await _service.UnsaveJobAsync(10, 999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSavedAsync_WhenSaved_ReturnsTrue()
    {
        _repoMock.Setup(r => r.IsSavedAsync(10, 5))
            .ReturnsAsync(true);

        var result = await _service.IsSavedAsync(10, 5);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsSavedAsync_WhenNotSaved_ReturnsFalse()
    {
        _repoMock.Setup(r => r.IsSavedAsync(10, 999))
            .ReturnsAsync(false);

        var result = await _service.IsSavedAsync(10, 999);

        result.Should().BeFalse();
    }

    private static SavedJob CreateSavedJob(long id, long jobSeekerProfileId, long opportunityId)
    {
        return new SavedJob
        {
            Id = id,
            JobSeekerProfileId = jobSeekerProfileId,
            OpportunityId = opportunityId,
            Notes = "Test notes",
            SavedAt = DateTime.UtcNow,
            Opportunity = new Opportunity
            {
                Id = opportunityId,
                Title = "Test Opportunity",
                Description = "Desc",
                OpportunityType = "Job",
                EmployerProfile = new EmployerProfile
                {
                    Id = 1,
                    CompanyName = "Test Co",
                    UserId = 1
                }
            }
        };
    }
}
