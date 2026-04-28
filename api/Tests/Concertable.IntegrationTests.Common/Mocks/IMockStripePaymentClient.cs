using Concertable.Payment.Application.Interfaces.Webhook;

namespace Concertable.IntegrationTests.Common.Mocks;

public interface IMockStripePaymentClient : IStripePaymentClient, IResettable
{
    string LastPaymentIntentId { get; }
    string LastEventId { get; }
    Dictionary<string, string> LastMetadata { get; }
}
