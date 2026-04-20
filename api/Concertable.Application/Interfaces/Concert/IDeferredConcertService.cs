using Concertable.Application.Responses;

namespace Concertable.Application.Interfaces.Concert;

public interface IDeferredConcertService
{
    Task<IAcceptOutcome> InitiateAsync(int applicationId);
    Task SettleAsync(int bookingId);
    Task<IFinishOutcome> FinishedAsync(int concertId, ManagerDto payer, ManagerDto payee, decimal amount);
}
