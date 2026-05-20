namespace Concertable.Shared.Email;

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string body, IReadOnlyList<EmailAttachment>? attachments = null);

    Task SendVerificationAsync(string toEmail, string token, string verifyBaseUrl, CancellationToken ct = default);
}
