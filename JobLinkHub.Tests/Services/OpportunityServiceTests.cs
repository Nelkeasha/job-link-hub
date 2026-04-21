using FluentAssertions;
using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Implementations;
using Moq;

namespace JobLinkHub.Tests.Services;

public class OpportunityServiceTests
{
    private readonly Mock<IOpportunityRepository> _repoMock;
    private readonly OpportunityService _service;

    public OpportunityServiceTests()
    {
        _repoMock = new Mock<IOpportunityRepository>();
        _service = new OpportunityService(_repoMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsFilteredResults()
    {
        var opportunities = new List<Opportunity>
        {
            CreateOpportunity(1, "C# Developer"),
        };
        _repoMock.Setup(r => r.SearchAsync("C#", null, null, null))
            .ReturnsAsync(opportunities);

        var result = (await _service.GetAllAsync("C#", null, null, null)).ToList();

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("C# Developer");
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsDto()
    {
        var opportunity = CreateOpportunity(1, "Test Job");
        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(opportunity);

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Job");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(999))
            .ReturnsAsync((Opportunity?)null);

        var result = await _service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedDto()
    {
        var dto = new CreateOpportunityDto
        {
            Title = "New Job",
            Description = "Description",
            OpportunityType = "Job",
            Location = "Remote",
            Status = "Active"
        };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Opportunity>()))
            .ReturnsAsync((Opportunity o) => o);
        _repoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<long>()))
            .ReturnsAsync(CreateOpportunity(1, "New Job"));

        var result = await _service.CreateAsync(1, dto);

        result.Should().NotBeNull();
        result.Title.Should().Be("New Job");
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Opportunity>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ByOwner_ReturnsUpdatedDto()
    {
        var existing = CreateOpportunity(1, "Old Title", employerProfileId: 5);
        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Opportunity>()))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var dto = new UpdateOpportunityDto
        {
            Title = "New Title",
            Description = "Updated",
            OpportunityType = "Job",
            Status = "Active"
        };

        var result = await _service.UpdateAsync(1, 5, dto);

        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
    }

    [Fact]
    public async Task UpdateAsync_ByNonOwner_ReturnsNull()
    {
        var existing = CreateOpportunity(1, "Title", employerProfileId: 5);
        _repoMock.Setup(r => r.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(existing);

        var dto = new UpdateOpportunityDto
        {
            Title = "New Title",
            Description = "X",
            OpportunityType = "Job",
            Status = "Active"
        };

        var result = await _service.UpdateAsync(1, 999, dto);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ByOwner_ReturnsTrue()
    {
        var existing = CreateOpportunity(1, "Title", employerProfileId: 5);
        _repoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);
        _repoMock.Setup(r => r.DeleteAsync(existing))
            .Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(1, 5);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ByNonOwner_ReturnsFalse()
    {
        var existing = CreateOpportunity(1, "Title", employerProfileId: 5);
        _repoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existing);

        var result = await _service.DeleteAsync(1, 999);

        result.Should().BeFalse();
    }

    private static Opportunity CreateOpportunity(long id, string title, long employerProfileId = 1)
    {
        return new Opportunity
        {
            Id = id,
            Title = title,
            Description = "Description",
            OpportunityType = "Job",
            Status = "Active",
            EmployerProfileId = employerProfileId,
            EmployerProfile = new EmployerProfile
            {
                Id = employerProfileId,
                CompanyName = "Test Co",
                UserId = 1
            },
            RequiredSkills = new List<OpportunitySkill>(),
            Applications = new List<Application>()
        };
    }
}
