namespace Concertable.Web.IntegrationTests.Infrastructure;

public class MockStripeClientSilent : IStripeClient
{
    public Task SendWebhookAsync() => Task.CompletedTask;
}
