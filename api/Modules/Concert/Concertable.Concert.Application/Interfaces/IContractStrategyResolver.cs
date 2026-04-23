namespace Concertable.Concert.Application.Interfaces;

internal interface IContractStrategyResolver<T> where T : IContractStrategy
{
    Task<T> ResolveForConcertAsync(int concertId);
    Task<T> ResolveForApplicationAsync(int applicationId);
    Task<T> ResolveForBookingAsync(int bookingId);
}
