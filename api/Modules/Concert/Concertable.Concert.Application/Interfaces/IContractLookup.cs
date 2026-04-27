using Concertable.Contract.Contracts;

namespace Concertable.Concert.Application.Interfaces;

internal interface IContractLookup
{
    Task<IContract> GetByApplicationIdAsync(int applicationId);
    Task<IContract> GetByBookingIdAsync(int bookingId);
    Task<IContract> GetByConcertIdAsync(int concertId);
}
