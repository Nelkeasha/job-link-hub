using JobLinkHub.Services.DTOs;

namespace JobLinkHub.Services.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetByUserAsync(long userId);
    Task<IEnumerable<NotificationDto>> GetUnreadByUserAsync(long userId);
    Task<int> GetUnreadCountAsync(long userId);
    Task MarkAsReadAsync(long id);
    Task MarkAllAsReadAsync(long userId);
    Task CreateAsync(long userId, string message, string type);
    Task NotifyApplicationStatusChangeAsync(long applicationId, string newStatus);
    Task NotifyNewApplicationAsync(long applicationId);
}
