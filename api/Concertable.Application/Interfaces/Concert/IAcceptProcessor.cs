namespace Concertable.Application.Interfaces.Concert;

public interface IAcceptProcessor
{
    Task AcceptAsync(int applicationId, string? paymentMethodId = null);
}
