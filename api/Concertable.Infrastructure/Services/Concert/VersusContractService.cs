using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;

namespace Concertable.Infrastructure.Services.Concert;

public class VersusContractService : IContractServiceStrategy
{
    public void ApplyChanges(ContractEntity existing, IContract dto)
    {
        var e = (VersusContractEntity)existing;
        var d = (VersusContractDto)dto;
        e.Update(d.Guarantee, d.ArtistDoorPercent, d.PaymentMethod);
    }
}
