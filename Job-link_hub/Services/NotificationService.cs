using JobLinkHub.API.Models.Notifications;
using JobLinkHub.Data;
using JobLinkHub.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.API.Services;

public class NotificationService(AppDbContext dbContext) : INotificationService
{
    public async Task<NotificationDto> CreateAsync(long userId, string message, string type)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            NotificationType = type
        };

        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();

        return ToDto(notification);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(long userId)
    {
        return await dbContext.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new NotificationDto
            {
                Id = x.Id,
                Message = x.Message,
                NotificationType = x.NotificationType,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<bool> MarkAsReadAsync(long userId, long notificationId)
    {
        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId);

        if (notification is null) return false;
        notification.IsRead = true;
        await dbContext.SaveChangesAsync();
        return true;
    }

    private static NotificationDto ToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Message = notification.Message,
            NotificationType = notification.NotificationType,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }
}
