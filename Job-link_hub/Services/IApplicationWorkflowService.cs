using JobLinkHub.Data.Entities;

namespace JobLinkHub.API.Services;

public interface IApplicationWorkflowService
{
    Task<(bool Success, string Message, Application? Application)> ApplyAsync(long userId, long opportunityId, string? coverLetter, string? resumeUsed);
    Task<(bool Success, string Message)> WithdrawAsync(long userId, long applicationId);
    Task<(bool Success, string Message, Application? Application)> UpdateStatusAsync(long userId, long applicationId, string newStatus, string? rejectionReason);
}
