namespace Concertable.Web.IntegrationTests.Infrastructure;

public interface IStripeClient
{
    Task SendWebhookAsync();
}
