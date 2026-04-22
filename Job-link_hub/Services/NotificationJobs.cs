using JobLinkHub.Data;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.API.Services;

public class NotificationJobs(
    AppDbContext dbContext,
    IEmailService emailService)
{
    public async Task SendUnreadDigestAsync()
    {
        var usersWithUnread = await dbContext.Notifications
            .Where(x => !x.IsRead)
            .GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var item in usersWithUnread)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == item.UserId);
            if (user?.Email is null) continue;

            var body = $"<p>You have {item.Count} unread notifications in JobLink Hub.</p>";
            await emailService.SendAsync(user.Email, "JobLink Hub Notification Digest", body);
        }
    }
}
