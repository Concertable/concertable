namespace Infrastructure.Interfaces;

public interface IWebhookService
{
    Task HandleAsync(string json, string stripeSignature);
}
