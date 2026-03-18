using Application.DTOs;
using Application.Interfaces.Concert;
using Concertable.Core.Entities.BookingContracts;
using Core.Enums;

namespace Application.Mappers;

public class DoorSplitContractMapper : IContractMapper
{
    public ContractType ContractType => ContractType.DoorSplit;

    public BookingContractEntity ToEntity(IBookingContract dto)
    {
        var d = (DoorSplitContractDto)dto;
        return new DoorSplitContractEntity
        {
            OpportunityId = d.OpportunityId,
            PaymentMethod = d.PaymentMethod,
            ArtistDoorPercent = d.ArtistDoorPercent
        };
    }

    public IBookingContract ToDto(BookingContractEntity entity)
    {
        var e = (DoorSplitContractEntity)entity;
        return new DoorSplitContractDto
        {
            Id = e.Id,
            OpportunityId = e.OpportunityId,
            PaymentMethod = e.PaymentMethod,
            ArtistDoorPercent = e.ArtistDoorPercent
        };
    }
}
