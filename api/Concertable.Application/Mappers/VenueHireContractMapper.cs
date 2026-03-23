using Application.DTOs;
using Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
using Core.Enums;

namespace Application.Mappers;

public class VenueHireContractMapper : IContractMapper
{
    public ContractType ContractType => ContractType.VenueHire;

    public ContractEntity ToEntity(IContract dto)
    {
        var d = (VenueHireContractDto)dto;
        return new VenueHireContractEntity
        {
            Id = d.Id,
            PaymentMethod = d.PaymentMethod,
            HireFee = d.HireFee
        };
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
