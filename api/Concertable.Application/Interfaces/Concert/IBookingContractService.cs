using Application.DTOs;
using Application.Interfaces.Concert;

namespace Application.Interfaces.Concert;

public interface IBookingContractService
{
    Task<IBookingContract> GetByOpportunityIdAsync(int opportunityId);
    Task CreateAsync(IBookingContract contract);
}
