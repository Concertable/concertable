using Concertable.Application.Interfaces;

namespace Concertable.Infrastructure.Services.Email;

public class FakeEmailService : IEmailService
{
    public Task SendEmailAsync(string toEmail, string subject, string body) =>
        Task.CompletedTask;

    public Task SendTicketsToEmailAsync(string toEmail, IEnumerable<int> ticketIds) =>
        Task.CompletedTask;
}
