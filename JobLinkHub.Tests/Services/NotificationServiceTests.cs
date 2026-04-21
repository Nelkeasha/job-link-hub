using FluentAssertions;
using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using JobLinkHub.Services.Implementations;
using JobLinkHub.Services.Interfaces;
using JobLinkHub.Tests.Helpers;
using Moq;

namespace JobLinkHub.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _repoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _repoMock = new Mock<INotificationRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        var context = TestDbContextFactory.Create();

        _service = new NotificationService(
            _repoMock.Object,
            _emailServiceMock.Object,
            context);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsNotifications()
    {
        var notifications = new List<Notification>
        {
            new() { Id = 1, UserId = 1, Message = "Test 1", NotificationType = "Info", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, UserId = 1, Message = "Test 2", NotificationType = "Info", CreatedAt = DateTime.UtcNow }
        };

        _repoMock.Setup(r => r.GetByUserAsync(1))
            .ReturnsAsync(notifications);

        var result = (await _service.GetByUserAsync(1)).ToList();

        result.Should().HaveCount(2);
        result[0].Message.Should().Be("Test 1");
    }

    [Fact]
    public async Task GetUnreadCountAsync_ReturnsCount()
    {
        _repoMock.Setup(r => r.GetUnreadCountAsync(1))
            .ReturnsAsync(5);

        var result = await _service.GetUnreadCountAsync(1);

        result.Should().Be(5);
    }

    [Fact]
    public async Task MarkAsReadAsync_UpdatesNotification()
    {
        _repoMock.Setup(r => r.MarkAsReadAsync(1))
            .Returns(Task.CompletedTask);

        await _service.MarkAsReadAsync(1);

        _repoMock.Verify(r => r.MarkAsReadAsync(1), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_PersistsNotification()
    {
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .ReturnsAsync((Notification n) => n);
        _repoMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        await _service.CreateAsync(1, "Test message", "Info");

        _repoMock.Verify(r => r.AddAsync(It.Is<Notification>(
            n => n.UserId == 1 && n.Message == "Test message" && n.NotificationType == "Info")),
            Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
