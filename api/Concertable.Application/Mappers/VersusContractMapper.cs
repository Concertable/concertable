using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
namespace Concertable.Application.Mappers;

public class VersusContractMapper : IContractMapper
{
    public ContractEntity ToEntity(IContract dto)
    {
        var d = (VersusContractDto)dto;
        return new VersusContractEntity
        {
            Id = d.Id,
            PaymentMethod = d.PaymentMethod,
            Guarantee = d.Guarantee,
            ArtistDoorPercent = d.ArtistDoorPercent
        };
    }

    public IContract ToDto(ContractEntity entity)
    {
        var e = (VersusContractEntity)entity;
        return new VersusContractDto
        {
            Id = e.Id,

            PaymentMethod = e.PaymentMethod,
            Guarantee = e.Guarantee,
            ArtistDoorPercent = e.ArtistDoorPercent
        };
    }
}
