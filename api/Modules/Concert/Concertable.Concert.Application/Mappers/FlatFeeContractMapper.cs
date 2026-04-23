using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Application.Interfaces;

namespace Concertable.Concert.Application.Mappers;

internal class FlatFeeContractMapper : IContractMapper
{
    public ContractEntity ToEntity(IContract dto)
    {
        var d = (FlatFeeContractDto)dto;
        return FlatFeeContractEntity.Create(d.Fee, d.PaymentMethod);
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
