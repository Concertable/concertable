using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Application.Interfaces;

namespace Concertable.Concert.Application.Mappers;

internal class DoorSplitContractMapper : IContractMapper
{
    public ContractEntity ToEntity(IContract dto)
    {
        var d = (DoorSplitContractDto)dto;
        return DoorSplitContractEntity.Create(d.ArtistDoorPercent, d.PaymentMethod);
    }

    public IContract ToDto(ContractEntity entity)
    {
        var e = (DoorSplitContractEntity)entity;
        return new DoorSplitContractDto
        {
            Id = e.Id,

            PaymentMethod = e.PaymentMethod,
            ArtistDoorPercent = e.ArtistDoorPercent
        };
    }
}
