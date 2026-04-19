using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Concert;

public interface IDeferredConcertService
{
    Task InitiateAsync(int applicationId, string? paymentMethodId = null);
    Task SettleAsync(int applicationId);
    Task FinishedAsync(int concertId, ManagerEntity payer, ManagerEntity payee, decimal amount);
}
