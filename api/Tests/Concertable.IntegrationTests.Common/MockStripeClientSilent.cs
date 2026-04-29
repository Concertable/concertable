namespace Concertable.IntegrationTests.Common;

public class MockStripeClientSilent : IStripeClient
{
    public Task SendWebhookAsync() => Task.CompletedTask;
}
