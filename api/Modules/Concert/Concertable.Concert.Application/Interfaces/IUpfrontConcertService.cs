using Concertable.Concert.Application.Responses;

namespace Concertable.Concert.Application.Interfaces;

internal interface IUpfrontConcertService
{
    Task<IAcceptOutcome> InitiateAsync(int applicationId, ManagerDto payer, ManagerDto payee, decimal amount, string? paymentMethodId = null);
    Task SettleAsync(int bookingId);
    Task<IFinishOutcome> FinishedAsync(int concertId);
}
