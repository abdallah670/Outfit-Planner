namespace OutfitPlanner.Application.Contracts.Infrastructure;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendVerificationEmailAsync(string to, string userName, string verificationToken);
    Task SendPasswordResetEmailAsync(string to, string userName, string resetToken);
}
