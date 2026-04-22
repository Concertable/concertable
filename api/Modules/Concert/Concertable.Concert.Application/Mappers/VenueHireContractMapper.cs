using Concertable.Concert.Application.DTOs;
using Concertable.Concert.Application.Interfaces;

namespace Concertable.Concert.Application.Mappers;

internal class VenueHireContractMapper : IContractMapper
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
