using Concertable.Application.Interfaces;
using Concertable.User.Contracts;
using Microsoft.Extensions.Logging;

namespace Concertable.Shared.Infrastructure.Services.Email;

public class FakeEmailService : IEmailService
{
    private readonly ILogger<FakeEmailService> logger;
    private readonly IUserModule userModule;

    public FakeEmailService(ILogger<FakeEmailService> logger, IUserModule userModule)
    {
        this.logger = logger;
        this.userModule = userModule;
    }

    public Task SendEmailAsync(string toEmail, string subject, string body)
    {
        logger.LogInformation("[FakeEmail] To: {Email} | Subject: {Subject}\n{Body}", toEmail, subject, body);
        return Task.CompletedTask;
    }

    public Task SendTicketsToEmailAsync(string toEmail, IEnumerable<Guid> ticketIds)
    {
        logger.LogInformation("[FakeEmail] Tickets to: {Email} | TicketIds: {Ids}", toEmail, string.Join(", ", ticketIds));
        return Task.CompletedTask;
    }

    public async Task SendVerificationAsync(string toEmail, string token, string verifyBaseUrl, CancellationToken ct = default)
    {
        logger.LogInformation("[FakeEmail] Auto-verifying {Email}", toEmail);
        await userModule.VerifyEmailWithTokenAsync(token, ct);
    }
}
