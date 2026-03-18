using Application.DTOs;
using Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
using Core.Enums;

namespace Application.Mappers;

public class FlatFeeContractMapper : IContractMapper
{
    public ContractType ContractType => ContractType.FlatFee;

    public ContractEntity ToEntity(IContract dto)
    {
        var d = (FlatFeeContractDto)dto;
        return new FlatFeeContractEntity
        {
            OpportunityId = d.OpportunityId,
            PaymentMethod = d.PaymentMethod,
            Fee = d.Fee
        };
    }

    public IContract ToDto(ContractEntity entity)
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
