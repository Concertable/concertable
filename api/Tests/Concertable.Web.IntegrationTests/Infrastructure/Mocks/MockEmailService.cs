using Concertable.Application.Interfaces;

namespace Concertable.Web.IntegrationTests.Infrastructure.Mocks;

public record SentEmail(string To, string Subject, string Body);

public class MockEmailService : IMockEmailService
{
    private readonly List<SentEmail> _sent = new();
    public IReadOnlyList<SentEmail> Sent => _sent;

    public Task SendEmailAsync(string toEmail, string subject, string body)
    {
        _sent.Add(new SentEmail(toEmail, subject, body));
        return Task.CompletedTask;
    }

    public Task SendTicketsToEmailAsync(string toEmail, IEnumerable<int> ticketIds) => Task.CompletedTask;

    public void Reset() => _sent.Clear();

    public string? ExtractToken(string email)
    {
        var msg = _sent.FirstOrDefault(m => m.To == email);
        if (msg is null) return null;

        var uri = msg.Body.Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.StartsWith("http"));
        if (uri is null) return null;

        var query = new Uri(uri).Query;
        var token = System.Web.HttpUtility.ParseQueryString(query)["token"];
        return token;
    }
}
