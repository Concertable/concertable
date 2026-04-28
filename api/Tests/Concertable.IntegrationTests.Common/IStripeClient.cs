namespace Concertable.IntegrationTests.Common;

public interface IStripeClient
{
    Task SendWebhookAsync();
}
