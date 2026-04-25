using JobLinkHub.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace JobLinkHub.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            var provider = _config["Email:Provider"] ?? "SendGrid";

            if (provider == "SendGrid")
            {
                var apiKey = _config["Email:SendGridApiKey"];
                if (string.IsNullOrEmpty(apiKey) || apiKey == "your-sendgrid-api-key-here")
                {
                    _logger.LogWarning("SendGrid API key not configured. Email to {To} with subject '{Subject}' was not sent.", to, subject);
                    return;
                }

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(
                    _config["Email:FromEmail"] ?? "noreply@joblinkhub.com",
                    _config["Email:FromName"] ?? "JobLink Hub");
                var toAddress = new EmailAddress(to);
                var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, null, htmlBody);
                var response = await client.SendEmailAsync(msg);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("SendGrid email failed with status {StatusCode}", response.StatusCode);
                }
            }
            else
            {
                await SendViaSmtpAsync(to, subject, htmlBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
        }
    }

    private async Task SendViaSmtpAsync(string to, string subject, string htmlBody)
    {
        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(
            _config["Email:SmtpHost"] ?? "smtp.gmail.com",
            int.Parse(_config["Email:SmtpPort"] ?? "587"),
            MailKit.Security.SecureSocketOptions.StartTls);

        var smtpUser = _config["Email:SmtpUser"];
        var smtpPass = _config["Email:SmtpPass"];
        if (!string.IsNullOrEmpty(smtpUser) && !string.IsNullOrEmpty(smtpPass))
            await client.AuthenticateAsync(smtpUser, smtpPass);

        var message = new MimeKit.MimeMessage();
        message.From.Add(new MimeKit.MailboxAddress(
            _config["Email:FromName"] ?? "JobLink Hub",
            _config["Email:FromEmail"] ?? "noreply@joblinkhub.com"));
        message.To.Add(MimeKit.MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new MimeKit.BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendEmailVerificationAsync(string to, string userName, string verificationLink)
    {
        var html = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <h2 style='color: #2563eb;'>Welcome to JobLink Hub!</h2>
            <p>Hi {userName},</p>
            <p>Thank you for registering. Please verify your email address by clicking the button below:</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{verificationLink}'
                   style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-size: 16px;'>
                    Verify Email Address
                </a>
            </div>
            <p>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #6b7280;'>{verificationLink}</p>
            <p>This link will expire in 24 hours.</p>
            <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 20px 0;' />
            <p style='color: #9ca3af; font-size: 12px;'>If you didn't create an account, please ignore this email.</p>
        </div>";

        await SendEmailAsync(to, "Verify your email - JobLink Hub", html);
    }

    public async Task SendPasswordResetAsync(string to, string userName, string resetLink)
    {
        var html = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <h2 style='color: #2563eb;'>Password Reset Request</h2>
            <p>Hi {userName},</p>
            <p>We received a request to reset your password. Click the button below to set a new password:</p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{resetLink}'
                   style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-size: 16px;'>
                    Reset Password
                </a>
            </div>
            <p>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #6b7280;'>{resetLink}</p>
            <p>This link will expire in 1 hour. If you didn't request a password reset, please ignore this email.</p>
            <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 20px 0;' />
            <p style='color: #9ca3af; font-size: 12px;'>JobLink Hub Team</p>
        </div>";

        await SendEmailAsync(to, "Reset your password - JobLink Hub", html);
    }

    public async Task SendApplicationStatusAsync(string to, string userName, string opportunityTitle, string status)
    {
        var statusColor = status.ToUpper() switch
        {
            "ACCEPTED" or "SHORTLISTED" => "#16a34a",
            "REJECTED" => "#dc2626",
            _ => "#2563eb"
        };

        var html = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <h2 style='color: #2563eb;'>Application Status Update</h2>
            <p>Hi {userName},</p>
            <p>Your application for <strong>{opportunityTitle}</strong> has been updated:</p>
            <div style='text-align: center; margin: 20px 0;'>
                <span style='background-color: {statusColor}; color: white; padding: 8px 20px; border-radius: 20px; font-size: 14px; font-weight: bold;'>
                    {status.ToUpper()}
                </span>
            </div>
            <p>Log in to your account to view more details.</p>
            <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 20px 0;' />
            <p style='color: #9ca3af; font-size: 12px;'>JobLink Hub Team</p>
        </div>";

        await SendEmailAsync(to, $"Application Update: {opportunityTitle} - JobLink Hub", html);
    }

    public async Task SendNewApplicationNotificationAsync(string to, string companyName, string candidateName, string opportunityTitle)
    {
        var html = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <h2 style='color: #2563eb;'>New Application Received</h2>
            <p>Hi {companyName},</p>
            <p><strong>{candidateName}</strong> has applied for your opportunity: <strong>{opportunityTitle}</strong></p>
            <p>Log in to your dashboard to review the application.</p>
            <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 20px 0;' />
            <p style='color: #9ca3af; font-size: 12px;'>JobLink Hub Team</p>
        </div>";

        await SendEmailAsync(to, $"New Application: {opportunityTitle} - JobLink Hub", html);
    }

    public async Task SendLoginOtpAsync(string to, string userName, string otp)
    {
        var html = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <h2 style='color: #2563eb;'>Your Login Code</h2>
            <p>Hi {userName},</p>
            <p>Use the code below to complete your sign-in. It expires in <strong>10 minutes</strong>.</p>
            <div style='text-align: center; margin: 30px 0;'>
                <span style='background-color: #f1f5f9; border: 2px dashed #2563eb; padding: 16px 40px; font-size: 2rem; font-weight: 800; letter-spacing: 0.4em; color: #0f172a; border-radius: 8px;'>
                    {otp}
                </span>
            </div>
            <p style='color: #6b7280; font-size: 0.875rem;'>If you didn't try to log in, please ignore this email.</p>
            <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 20px 0;' />
            <p style='color: #9ca3af; font-size: 12px;'>JobLink Hub Team</p>
        </div>";

        await SendEmailAsync(to, "Your JobLink Hub login code", html);
    }

    public async Task SendOpportunityMatchAsync(string to, string userName, List<string> opportunityTitles)
    {
        var listItems = string.Join("", opportunityTitles.Select(t => $"<li style='margin: 8px 0;'>{t}</li>"));
        var html = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
            <h2 style='color: #2563eb;'>New Opportunities Match Your Skills!</h2>
            <p>Hi {userName},</p>
            <p>We found opportunities that match your profile:</p>
            <ul style='padding-left: 20px;'>
                {listItems}
            </ul>
            <p>Log in to your account to view details and apply.</p>
            <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 20px 0;' />
            <p style='color: #9ca3af; font-size: 12px;'>JobLink Hub Team</p>
        </div>";

        await SendEmailAsync(to, "New Matching Opportunities - JobLink Hub", html);
    }
}
