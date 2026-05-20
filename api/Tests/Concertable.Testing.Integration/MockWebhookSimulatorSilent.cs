namespace Concertable.Testing.Integration;

public class MockWebhookSimulatorSilent : IWebhookSimulator
{
    public Task SendWebhookAsync() => Task.CompletedTask;
}
