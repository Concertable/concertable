using Concertable.Application.Responses;

namespace Concertable.Application.Interfaces.Concert;

public interface IUpfrontConcertService
{
    Task<IAcceptOutcome> InitiateAsync(int applicationId, ManagerDto payer, ManagerDto payee, decimal amount, string? paymentMethodId = null);
    Task SettleAsync(int bookingId);
    Task<IFinishOutcome> FinishedAsync(int concertId);
}
