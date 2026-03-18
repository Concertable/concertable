using Application.DTOs;
using Application.Interfaces.Concert;
using Concertable.Core.Entities.BookingContracts;
using Core.Enums;

namespace Application.Mappers;

public class VersusContractMapper : IContractMapper
{
    public ContractType ContractType => ContractType.Versus;

    public BookingContractEntity ToEntity(IBookingContract dto)
    {
        var d = (VersusContractDto)dto;
        return new VersusContractEntity
        {
            OpportunityId = d.OpportunityId,
            PaymentMethod = d.PaymentMethod,
            Guarantee = d.Guarantee,
            ArtistDoorPercent = d.ArtistDoorPercent
        };
    }

    public IBookingContract ToDto(BookingContractEntity entity)
    {
        var e = (VersusContractEntity)entity;
        return new VersusContractDto
        {
            Id = e.Id,
            OpportunityId = e.OpportunityId,
            PaymentMethod = e.PaymentMethod,
            Guarantee = e.Guarantee,
            ArtistDoorPercent = e.ArtistDoorPercent
        };
    }
}
