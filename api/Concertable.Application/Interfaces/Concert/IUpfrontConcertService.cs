using Concertable.Application.Responses;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Concert;

public interface IUpfrontConcertService
{
    Task<IAcceptOutcome> InitiateAsync(int applicationId, ManagerEntity payer, ManagerEntity payee, decimal amount, string? paymentMethodId = null);
    Task SettleAsync(int bookingId);
    Task<IFinishOutcome> FinishedAsync(int concertId);
}
