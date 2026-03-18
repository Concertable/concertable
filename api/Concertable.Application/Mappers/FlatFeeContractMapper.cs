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
            Id = d.Id,
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

            PaymentMethod = e.PaymentMethod,
            Fee = e.Fee
        };
    }
}
