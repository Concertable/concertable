namespace Concertable.Web.IntegrationTests.Infrastructure;

public interface IFakeStripeClient
{
    Task SendWebhookAsync();
}
