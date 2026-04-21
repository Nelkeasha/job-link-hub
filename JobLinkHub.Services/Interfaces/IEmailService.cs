namespace JobLinkHub.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
    Task SendEmailVerificationAsync(string to, string userName, string verificationLink);
    Task SendPasswordResetAsync(string to, string userName, string resetLink);
    Task SendApplicationStatusAsync(string to, string userName, string opportunityTitle, string status);
    Task SendNewApplicationNotificationAsync(string to, string companyName, string candidateName, string opportunityTitle);
    Task SendOpportunityMatchAsync(string to, string userName, List<string> opportunityTitles);
}
