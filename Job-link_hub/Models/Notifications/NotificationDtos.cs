namespace JobLinkHub.API.Models.Notifications;

public class NotificationDto
{
    public long Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateNotificationRequest
{
    public long UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string NotificationType { get; set; } = "GENERAL";
}
