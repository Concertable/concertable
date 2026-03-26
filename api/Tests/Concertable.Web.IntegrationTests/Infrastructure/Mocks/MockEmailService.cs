using Application.Interfaces;

namespace Concertable.Web.IntegrationTests.Infrastructure.Mocks;

public class MockEmailService : IEmailService
{
    public Task SendTicketsToEmailAsync(string toEmail, IEnumerable<int> ticketIds) => Task.CompletedTask;
    public Task SendEmailAsync(string toEmail, string subject, string body) => Task.CompletedTask;
}
