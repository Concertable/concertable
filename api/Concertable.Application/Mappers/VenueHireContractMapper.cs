using Application.DTOs;
using Application.Interfaces.Concert;
using Concertable.Core.Entities.BookingContracts;
using Core.Enums;

namespace Application.Mappers;

public class VenueHireContractMapper : IContractMapper
{
    public ContractType ContractType => ContractType.VenueHire;

    public BookingContractEntity ToEntity(IBookingContract dto)
    {
        var d = (VenueHireContractDto)dto;
        return new VenueHireContractEntity
        {
            OpportunityId = d.OpportunityId,
            PaymentMethod = d.PaymentMethod,
            HireFee = d.HireFee
        };
    }

    public IBookingContract ToDto(BookingContractEntity entity)
    {
        var e = (VenueHireContractEntity)entity;
        return new VenueHireContractDto
        {
            Id = e.Id,
            OpportunityId = e.OpportunityId,
            PaymentMethod = e.PaymentMethod,
            HireFee = e.HireFee
        };
    }
}
