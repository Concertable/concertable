using Application.DTOs;
using Application.Interfaces.Concert;
using Concertable.Core.Entities.BookingContracts;
using Core.Enums;

namespace Application.Mappers;

public class FlatFeeContractMapper : IContractMapper
{
    public ContractType ContractType => ContractType.FlatFee;

    public BookingContractEntity ToEntity(IBookingContract dto)
    {
        var d = (FlatFeeContractDto)dto;
        return new FlatFeeContractEntity
        {
            OpportunityId = d.OpportunityId,
            PaymentMethod = d.PaymentMethod,
            Fee = d.Fee
        };
    }

    public IBookingContract ToDto(BookingContractEntity entity)
    {
        var e = (FlatFeeContractEntity)entity;
        return new FlatFeeContractDto
        {
            Id = e.Id,
            OpportunityId = e.OpportunityId,
            PaymentMethod = e.PaymentMethod,
            Fee = e.Fee
        };
    }
}
