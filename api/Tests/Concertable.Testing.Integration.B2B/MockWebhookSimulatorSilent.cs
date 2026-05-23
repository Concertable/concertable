namespace Concertable.Testing.Integration.B2B;

public class MockWebhookSimulatorSilent : IWebhookSimulator
{
    public Task SendWebhookAsync() => Task.CompletedTask;
}
