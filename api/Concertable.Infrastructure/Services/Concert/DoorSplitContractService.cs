using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;

namespace Concertable.Infrastructure.Services.Concert;

public class DoorSplitContractService : IContractServiceStrategy
{
    public void ApplyChanges(ContractEntity existing, IContract dto)
    {
        var e = (DoorSplitContractEntity)existing;
        var d = (DoorSplitContractDto)dto;
        e.PaymentMethod = d.PaymentMethod;
        e.ArtistDoorPercent = d.ArtistDoorPercent;
    }
}
