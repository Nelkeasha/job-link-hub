using JobLinkHub.API.Models.Notifications;

namespace JobLinkHub.API.Services;

public interface INotificationService
{
    Task<NotificationDto> CreateAsync(long userId, string message, string type);
    Task<IReadOnlyList<NotificationDto>> GetForUserAsync(long userId);
    Task<bool> MarkAsReadAsync(long userId, long notificationId);
}
