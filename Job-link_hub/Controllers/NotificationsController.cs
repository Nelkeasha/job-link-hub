using JobLinkHub.API.Models.Notifications;
using JobLinkHub.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobLinkHub.API.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController(
    INotificationService notificationService,
    IUserContextService userContextService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var user = await userContextService.GetCurrentUserAsync(User);
        if (user is null) return Unauthorized();

        var result = await notificationService.GetForUserAsync(user.Id);
        return Ok(result);
    }

    [HttpPut("{id:long}/read")]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        var user = await userContextService.GetCurrentUserAsync(User);
        if (user is null) return Unauthorized();

        var updated = await notificationService.MarkAsReadAsync(user.Id, id);
        return updated ? Ok("Notification marked as read.") : NotFound("Notification not found.");
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create(CreateNotificationRequest request)
    {
        var result = await notificationService.CreateAsync(request.UserId, request.Message, request.NotificationType);
        return Ok(result);
    }
}
