namespace Concertable.IntegrationTests.Common;

public interface IWebhookSimulator
{
    Task SendWebhookAsync();
}
