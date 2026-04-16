using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;

namespace Concertable.Infrastructure.Services.Concert;

public class FlatFeeContractService : IContractServiceStrategy
{
    public void ApplyChanges(ContractEntity existing, IContract dto)
    {
        var e = (FlatFeeContractEntity)existing;
        var d = (FlatFeeContractDto)dto;
        e.Update(d.Fee, d.PaymentMethod);
    }
}
