using Application.Interfaces;
using Concertable.Core.Entities.BookingContracts;

namespace Application.Interfaces.Concert;

public interface IBookingContractRepository : IRepository<BookingContractEntity>
{
    Task<BookingContractEntity?> GetByOpportunityIdAsync(int opportunityId);
}
