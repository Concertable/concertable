namespace Infrastructure.Interfaces;

public interface IWebhookStrategyFactory
{
    IWebhookStrategy Create(string type);
}
