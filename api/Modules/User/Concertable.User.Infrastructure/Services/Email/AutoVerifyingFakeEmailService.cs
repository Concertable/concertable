using Concertable.Shared.Email;
using Concertable.User.Contracts;
using Microsoft.Extensions.Logging;

namespace Concertable.User.Infrastructure.Services.Email;

internal sealed class AutoVerifyingFakeEmailService : IEmailService
{
    private readonly ILogger<AutoVerifyingFakeEmailService> logger;
    private readonly IUserModule userModule;

    public AutoVerifyingFakeEmailService(ILogger<AutoVerifyingFakeEmailService> logger, IUserModule userModule)
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
