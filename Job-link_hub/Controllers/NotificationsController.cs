using JobLinkHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notifications = await _notificationService.GetByUserAsync(userId);
        return Ok(notifications);
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notifications = await _notificationService.GetUnreadByUserAsync(userId);
        return Ok(notifications);
    }

    [HttpGet("unread/count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(new { count });
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return Ok(new { message = "Notification marked as read" });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(new { message = "All notifications marked as read" });
    }
}
