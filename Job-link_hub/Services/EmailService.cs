using Serilog;

namespace JobLinkHub.API.Services;

public class EmailService : IEmailService
{
    public Task SendAsync(string toEmail, string subject, string bodyHtml)
    {
        Log.Information("Email queued (dev stub). To: {ToEmail}, Subject: {Subject}, BodyLength: {BodyLength}",
            toEmail, subject, bodyHtml.Length);
        return Task.CompletedTask;
    }
}
