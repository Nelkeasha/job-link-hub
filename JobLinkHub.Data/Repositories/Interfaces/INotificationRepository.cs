using JobLinkHub.Data.Entities;

namespace JobLinkHub.Data.Repositories.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserAsync(long userId);
    Task<IEnumerable<Notification>> GetUnreadByUserAsync(long userId);
    Task<int> GetUnreadCountAsync(long userId);
    Task MarkAsReadAsync(long id);
    Task MarkAllAsReadAsync(long userId);
}
