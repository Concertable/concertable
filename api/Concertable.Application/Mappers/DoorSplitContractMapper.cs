using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
namespace Concertable.Application.Mappers;

public class DoorSplitContractMapper : IContractMapper
{
    public ContractEntity ToEntity(IContract dto)
    {
        var d = (DoorSplitContractDto)dto;
        return new DoorSplitContractEntity
        {
            Id = d.Id,
            PaymentMethod = d.PaymentMethod,
            ArtistDoorPercent = d.ArtistDoorPercent
        };
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
