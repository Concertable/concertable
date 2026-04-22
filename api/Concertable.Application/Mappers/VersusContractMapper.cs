using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
namespace Concertable.Application.Mappers;

public class VersusContractMapper : IContractMapper
{
    public ContractEntity ToEntity(IContract dto)
    {
        var d = (VersusContractDto)dto;
        return VersusContractEntity.Create(d.Guarantee, d.ArtistDoorPercent, d.PaymentMethod);
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
