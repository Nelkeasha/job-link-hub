using JobLinkHub.Data;
using JobLinkHub.Data.Entities;
using JobLinkHub.Data.Repositories.Interfaces;
using JobLinkHub.Services.DTOs;
using JobLinkHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JobLinkHub.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _context;

    public NotificationService(
        INotificationRepository repo,
        IEmailService emailService,
        AppDbContext context)
    {
        _repo = repo;
        _emailService = emailService;
        _context = context;
    }

    public async Task<IEnumerable<NotificationDto>> GetByUserAsync(long userId)
    {
        var notifications = await _repo.GetByUserAsync(userId);
        return notifications.Select(MapToDto);
    }

    public async Task<IEnumerable<NotificationDto>> GetUnreadByUserAsync(long userId)
    {
        var notifications = await _repo.GetUnreadByUserAsync(userId);
        return notifications.Select(MapToDto);
    }

    public async Task<int> GetUnreadCountAsync(long userId)
    {
        return await _repo.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(long id)
    {
        await _repo.MarkAsReadAsync(id);
    }

    public async Task MarkAllAsReadAsync(long userId)
    {
        await _repo.MarkAllAsReadAsync(userId);
    }

    public async Task CreateAsync(long userId, string message, string type)
    {
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            NotificationType = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(notification);
        await _repo.SaveChangesAsync();
    }

    public async Task NotifyApplicationStatusChangeAsync(long applicationId, string newStatus)
    {
        var application = await _context.Applications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o.EmployerProfile)
            .Include(a => a.JobSeekerProfile)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application?.JobSeekerProfile?.User == null) return;

        var user = application.JobSeekerProfile.User;
        var opportunityTitle = application.Opportunity?.Title ?? "Unknown";

        // Create in-app notification
        await CreateAsync(
            user.Id,
            $"Your application for '{opportunityTitle}' has been updated to: {newStatus}",
            "ApplicationStatus");

        // Send email
        await _emailService.SendApplicationStatusAsync(
            user.Email!,
            $"{user.FirstName} {user.LastName}",
            opportunityTitle,
            newStatus);
    }

    public async Task NotifyNewApplicationAsync(long applicationId)
    {
        var application = await _context.Applications
            .Include(a => a.Opportunity)
                .ThenInclude(o => o.EmployerProfile)
                    .ThenInclude(ep => ep.User)
            .Include(a => a.JobSeekerProfile)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application?.Opportunity?.EmployerProfile?.User == null) return;

        var employer = application.Opportunity.EmployerProfile;
        var employerUser = employer.User;
        var candidateUser = application.JobSeekerProfile?.User;
        var candidateName = candidateUser != null
            ? $"{candidateUser.FirstName} {candidateUser.LastName}"
            : "A candidate";

        // Create in-app notification
        await CreateAsync(
            employerUser.Id,
            $"{candidateName} has applied for '{application.Opportunity.Title}'",
            "NewApplication");

        // Send email
        await _emailService.SendNewApplicationNotificationAsync(
            employerUser.Email!,
            employer.CompanyName,
            candidateName,
            application.Opportunity.Title);
    }

    private static NotificationDto MapToDto(Notification n) => new()
    {
        Id = n.Id,
        Message = n.Message,
        NotificationType = n.NotificationType,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt
    };
}
