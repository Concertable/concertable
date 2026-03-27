using Application.DTOs;
using Application.Mappers;
using Concertable.Core.Entities.Contracts;
using Core.Enums;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Mappers;

public class FlatFeeContractMapperTests
{
    private readonly FlatFeeContractMapper sut = new();

    [Fact]
    public void ToEntity_ShouldMapAllFields()
    {
        var dto = new FlatFeeContractDto
        {
            Id = 1,
            PaymentMethod = PaymentMethod.Cash,
            Fee = 500
        };

        var result = (FlatFeeContractEntity)sut.ToEntity(dto);

        Assert.Equal(dto.Id, result.Id);
        Assert.Equal(dto.PaymentMethod, result.PaymentMethod);
        Assert.Equal(dto.Fee, result.Fee);
    }

    [Fact]
    public void ToDto_ShouldMapAllFields()
    {
        var entity = new FlatFeeContractEntity
        {
            Id = 1,
            PaymentMethod = PaymentMethod.Cash,
            Fee = 500
        };

        var result = (FlatFeeContractDto)sut.ToDto(entity);

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.PaymentMethod, result.PaymentMethod);
        Assert.Equal(entity.Fee, result.Fee);
    }
}
