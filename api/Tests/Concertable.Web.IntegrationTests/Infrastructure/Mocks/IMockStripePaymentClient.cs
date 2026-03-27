using Infrastructure.Interfaces;

namespace Concertable.Web.IntegrationTests.Infrastructure.Mocks;

public interface IMockStripePaymentClient : IStripePaymentClient, IResettable
{
    string LastPaymentIntentId { get; }
    Dictionary<string, string> LastMetadata { get; }
}
