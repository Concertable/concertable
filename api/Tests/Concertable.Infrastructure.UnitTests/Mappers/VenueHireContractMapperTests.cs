using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Core.Enums;
using Concertable.Seeding;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Mappers;

public class VenueHireContractMapperTests
{
    private readonly VenueHireContractMapper sut = new();

    [Fact]
    public void ToEntity_ShouldMapAllFields()
    {
        var dto = new VenueHireContractDto { Id = 1, PaymentMethod = PaymentMethod.Cash, HireFee = 300 };

        var result = (VenueHireContractEntity)sut.ToEntity(dto);

        Assert.Equal(dto.PaymentMethod, result.PaymentMethod);
        Assert.Equal(dto.HireFee, result.HireFee);
    }

    [Fact]
    public void ToDto_ShouldMapAllFields()
    {
        var entity = VenueHireContractEntity.Create(300, PaymentMethod.Cash);

        var result = (VenueHireContractDto)sut.ToDto(entity);

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.PaymentMethod, result.PaymentMethod);
        Assert.Equal(entity.HireFee, result.HireFee);
    }
}
