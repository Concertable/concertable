using Concertable.Application.Interfaces;

namespace Concertable.Web.IntegrationTests.Infrastructure.Mocks;

public interface IMockEmailService : IEmailService, IResettable
{
    IReadOnlyList<SentEmail> Sent { get; }
    string? ExtractToken(string email);
}
