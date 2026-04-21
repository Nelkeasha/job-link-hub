using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.Data.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Notification>> GetByUserAsync(long userId)
    {
        return await _dbSet
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetUnreadByUserAsync(long userId)
    {
        return await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(long userId)
    {
        return await _dbSet
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(long id)
    {
        var notification = await _dbSet.FindAsync(id);
        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(long userId)
    {
        var unread = await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unread)
            notification.IsRead = true;

        await _context.SaveChangesAsync();
    }
}
