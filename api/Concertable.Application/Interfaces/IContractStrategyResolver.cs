using Concertable.Application.Interfaces.Concert;

namespace Concertable.Application.Interfaces;

public interface IContractStrategyResolver<T> where T : IContractStrategy
{
    Task<T> ResolveForConcertAsync(int concertId);
    Task<T> ResolveForApplicationAsync(int applicationId);
    Task<T> ResolveForBookingAsync(int bookingId);
}
