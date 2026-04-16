using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;

namespace Concertable.Infrastructure.Services.Concert;

public class VenueHireContractService : IContractServiceStrategy
{
    public void ApplyChanges(ContractEntity existing, IContract dto)
    {
        var e = (VenueHireContractEntity)existing;
        var d = (VenueHireContractDto)dto;
        e.Update(d.HireFee, d.PaymentMethod);
    }
}
