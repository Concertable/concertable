using Application.DTOs;
using Application.Interfaces.Concert;
using Concertable.Core.Entities.BookingContracts;
using Core.Entities;

namespace Application.Mappers;

public static class BookingContractMappers
{
    public static IBookingContract ToDto(this BookingContractEntity contract) => contract switch
    {
        FlatFeeContractEntity f => new FlatFeeContractDto
        {
            Id = f.Id,
            OpportunityId = f.OpportunityId,
            PaymentMethod = f.PaymentMethod,
            Fee = f.Fee
        },
        DoorSplitContractEntity d => new DoorSplitContractDto
        {
            Id = d.Id,
            OpportunityId = d.OpportunityId,
            PaymentMethod = d.PaymentMethod,
            ArtistDoorPercent = d.ArtistDoorPercent
        },
        VersusContractEntity v => new VersusContractDto
        {
            Id = v.Id,
            OpportunityId = v.OpportunityId,
            PaymentMethod = v.PaymentMethod,
            Guarantee = v.Guarantee,
            ArtistDoorPercent = v.ArtistDoorPercent
        },
        VenueHireContractEntity vh => new VenueHireContractDto
        {
            Id = vh.Id,
            OpportunityId = vh.OpportunityId,
            PaymentMethod = vh.PaymentMethod,
            HireFee = vh.HireFee
        },
        _ => throw new InvalidOperationException($"Unknown contract type: {contract.GetType().Name}")
    };

    public static BookingContractEntity ToEntity(this IBookingContract contract) => contract switch
    {
        FlatFeeContractDto f => new FlatFeeContractEntity
        {
            OpportunityId = f.OpportunityId,
            PaymentMethod = f.PaymentMethod,
            Fee = f.Fee
        },
        DoorSplitContractDto d => new DoorSplitContractEntity
        {
            OpportunityId = d.OpportunityId,
            PaymentMethod = d.PaymentMethod,
            ArtistDoorPercent = d.ArtistDoorPercent
        },
        VersusContractDto v => new VersusContractEntity
        {
            OpportunityId = v.OpportunityId,
            PaymentMethod = v.PaymentMethod,
            Guarantee = v.Guarantee,
            ArtistDoorPercent = v.ArtistDoorPercent
        },
        VenueHireContractDto vh => new VenueHireContractEntity
        {
            OpportunityId = vh.OpportunityId,
            PaymentMethod = vh.PaymentMethod,
            HireFee = vh.HireFee
        },
        _ => throw new InvalidOperationException($"Unknown contract type: {contract.GetType().Name}")
    };
}
