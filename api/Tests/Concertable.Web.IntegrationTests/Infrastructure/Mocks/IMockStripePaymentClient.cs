using Concertable.Payment.Application.Interfaces.Webhook;

namespace Concertable.Web.IntegrationTests.Infrastructure.Mocks;

internal interface IMockStripePaymentClient : IStripePaymentClient, IResettable
{
    string LastPaymentIntentId { get; }
    string LastEventId { get; }
    Dictionary<string, string> LastMetadata { get; }
}
