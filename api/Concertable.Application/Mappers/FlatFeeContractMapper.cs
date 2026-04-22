using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
namespace Concertable.Application.Mappers;

public class FlatFeeContractMapper : IContractMapper
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
