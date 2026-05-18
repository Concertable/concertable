using Concertable.Application.DTOs;

namespace Concertable.Application.Interfaces;

public interface IEmailService
{
    Task SendTicketsToEmailAsync(string toEmail, IEnumerable<Guid> ticketIds);
    Task SendEmailAsync(string toEmail, string subject, string body);

    Task SendVerificationAsync(string toEmail, string token, string verifyBaseUrl, CancellationToken ct = default);
}
