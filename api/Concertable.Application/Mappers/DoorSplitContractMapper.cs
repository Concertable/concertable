using Application.DTOs;
using Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
using Core.Enums;

namespace Application.Mappers;

public class DoorSplitContractMapper : IContractMapper
{
    public ContractType ContractType => ContractType.DoorSplit;

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
