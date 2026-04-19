using Concertable.Application.Responses;
using Concertable.Core.Entities;

namespace Concertable.Application.Interfaces.Concert;

public interface IDeferredConcertService
{
    Task<IAcceptOutcome> InitiateAsync(int applicationId);
    Task SettleAsync(int applicationId);
    Task<IFinishOutcome> FinishedAsync(int concertId, ManagerEntity payer, ManagerEntity payee, decimal amount);
}
