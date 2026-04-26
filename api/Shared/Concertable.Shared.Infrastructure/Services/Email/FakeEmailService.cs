using Concertable.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Concertable.Shared.Infrastructure.Services.Email;

public class FakeEmailService : IEmailService
{
    private readonly ILogger<FakeEmailService> logger;

    public FakeEmailService(ILogger<FakeEmailService> logger)
    {
        this.logger = logger;
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
}
