using Concertable.Core.Enums;
using Stripe;

namespace Concertable.Infrastructure.UnitTests.Services.Webhook;

public static class WebhookProcessorBuilders
{
    public static Event BuildEvent(string eventId, IHasObject obj) => new()
    {
        Id = eventId,
        Data = new EventData { Object = obj }
    };

    public static Event BuildEvent(string eventId, PaymentIntent intent) => new()
    {
        Id = eventId,
        Data = new EventData { Object = intent }
    };

    public static PaymentIntent BuildIntent(string status, WebhookType type) => new()
    {
        Id = "pi_test",
        Status = status,
        Metadata = new Dictionary<string, string> { { "type", type.ToString().ToLower() } }
    };
}
