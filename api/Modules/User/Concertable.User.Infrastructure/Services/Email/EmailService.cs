using Concertable.DataAccess;

namespace Concertable.User.Infrastructure.Services.Email;

internal class EmailService : IEmailService
{
    public Task SendEmailAsync(string toEmail, string subject, string body)
        => throw new NotImplementedException();

    public Task SendTicketsToEmailAsync(string toEmail, IEnumerable<Guid> ticketIds)
        => throw new NotImplementedException();

    public Task SendVerificationAsync(string toEmail, string token, string verifyBaseUrl, CancellationToken ct = default)
        => throw new NotImplementedException();
}
