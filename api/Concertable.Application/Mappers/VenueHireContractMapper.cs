using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
namespace Concertable.Application.Mappers;

public class VenueHireContractMapper : IContractMapper
{
    public ContractEntity ToEntity(IContract dto)
    {
        var d = (VenueHireContractDto)dto;
        return VenueHireContractEntity.Create(d.HireFee, d.PaymentMethod);
    }

    public IContract ToDto(ContractEntity entity)
    {
        var e = (VenueHireContractEntity)entity;
        return new VenueHireContractDto
        {
            Id = e.Id,

            PaymentMethod = e.PaymentMethod,
            HireFee = e.HireFee
        };
    }
}
