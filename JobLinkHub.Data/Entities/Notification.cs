namespace JobLinkHub.Data.Entities;

public class Notification
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}