namespace Concertable.IntegrationTests.Common;

public class MockWebhookSimulatorSilent : IWebhookSimulator
{
    public Task SendWebhookAsync() => Task.CompletedTask;
}
