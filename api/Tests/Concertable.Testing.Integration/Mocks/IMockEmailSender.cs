using Concertable.Shared.Email;

namespace Concertable.Testing.Integration.Mocks;

public interface IMockEmailSender : IEmailSender, IResettable
{
    IReadOnlyList<SentEmail> Sent { get; }
    string? ExtractToken(string email);
}
