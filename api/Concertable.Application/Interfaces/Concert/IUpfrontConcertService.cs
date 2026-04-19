using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Concert;

public interface IUpfrontConcertService
{
    Task InitiateAsync(int applicationId, ManagerEntity payer, ManagerEntity payee, decimal amount, string? paymentMethodId = null);
    Task SettleAsync(int applicationId);
    Task FinishedAsync(int concertId);
}
