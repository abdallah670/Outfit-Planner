using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutfitPlanner.Application.Contracts.Infrastructure;
using OutfitPlanner.Application.Models;
using System.Net;
using System.Net.Mail;

namespace OutfitPlanner.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        // Validate email settings
        if (string.IsNullOrWhiteSpace(_emailSettings.SmtpUsername) || string.IsNullOrWhiteSpace(_emailSettings.SmtpPassword))
        {
            _logger.LogError("Email settings not configured. Please set EmailSettings__SmtpUsername and EmailSettings__SmtpPassword environment variables.");
            throw new InvalidOperationException("Email service is not configured. Please contact the administrator.");
        }

        try
        {
            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.From, _emailSettings.DisplayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            message.To.Add(to);

            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort);
            client.EnableSsl = _emailSettings.EnableSsl;
            client.Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            throw;
        }
    }

    public async Task SendVerificationEmailAsync(string to, string userName, string verificationToken)
    {
        var subject = "Verify your Outfit Planner account";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #3f51b5; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; margin: 20px 0; }}
        .button {{ background-color: #3f51b5; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0; }}
        .token {{ background-color: #e0e0e0; padding: 15px; font-family: monospace; font-size: 16px; text-align: center; margin: 20px 0; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to Outfit Planner!</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>Thank you for signing up. Please verify your email address to complete your registration.</p>
            <p>Your verification code is:</p>
            <div class='token'>{verificationToken}</div>
            <p>Enter this code on the verification page, or click the button below:</p>
            <a href='https://localhost:4200/verify-email?token={verificationToken}&email={WebUtility.UrlEncode(to)}' class='button'>Verify Email</a>
            <p style='margin-top: 30px;'>This code will expire in 24 hours.</p>
        </div>
        <div class='footer'>
            <p>If you didn't create an account, you can safely ignore this email.</p>
            <p>&copy; 2024 Outfit Planner. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(to, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string to, string userName, string resetToken)
    {
        var subject = "Reset your Outfit Planner password";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #ff5722; color: white; padding: 20px; text-align: center; }}
        .content {{ background-color: #f9f9f9; padding: 30px; margin: 20px 0; }}
        .button {{ background-color: #ff5722; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin: 20px 0; }}
        .token {{ background-color: #e0e0e0; padding: 15px; font-family: monospace; font-size: 16px; text-align: center; margin: 20px 0; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
        .footer {{ text-align: center; color: #666; font-size: 12px; margin-top: 30px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <h2>Hello {userName},</h2>
            <p>We received a request to reset your password. Use the code below to reset it:</p>
            <div class='token'>{resetToken}</div>
            <p>Or click the button below to reset your password:</p>
            <a href='https://localhost:4200/reset-password?token={resetToken}&email={WebUtility.UrlEncode(to)}' class='button'>Reset Password</a>
            <div class='warning'>
                <strong>Security Notice:</strong> This code will expire in 1 hour. If you didn't request this, please ignore this email.
            </div>
        </div>
        <div class='footer'>
            <p>&copy; 2024 Outfit Planner. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(to, subject, body);
    }
}
