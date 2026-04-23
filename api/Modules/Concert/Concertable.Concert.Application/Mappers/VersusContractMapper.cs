using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Application.Interfaces;

namespace Concertable.Concert.Application.Mappers;

internal class VersusContractMapper : IContractMapper
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
