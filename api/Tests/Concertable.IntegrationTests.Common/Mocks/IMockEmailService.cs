using Concertable.DataAccess;

namespace Concertable.IntegrationTests.Common.Mocks;

public interface IMockEmailService : IEmailService, IResettable
{
    IReadOnlyList<SentEmail> Sent { get; }
    string? ExtractToken(string email);
}
