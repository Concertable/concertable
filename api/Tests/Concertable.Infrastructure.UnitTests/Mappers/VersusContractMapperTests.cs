using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Mappers;

public class VersusContractMapperTests
{
    private readonly VersusContractMapper sut = new();

    [Fact]
    public void ToEntity_ShouldMapAllFields()
    {
        var dto = new VersusContractDto
        {
            Id = 1,
            PaymentMethod = PaymentMethod.Cash,
            Guarantee = 200,
            ArtistDoorPercent = 50
        };

        var result = (VersusContractEntity)sut.ToEntity(dto);

        Assert.Equal(dto.Id, result.Id);
        Assert.Equal(dto.PaymentMethod, result.PaymentMethod);
        Assert.Equal(dto.Guarantee, result.Guarantee);
        Assert.Equal(dto.ArtistDoorPercent, result.ArtistDoorPercent);
    }

    [Fact]
    public void ToDto_ShouldMapAllFields()
    {
        var entity = new VersusContractEntity
        {
            Id = 1,
            PaymentMethod = PaymentMethod.Cash,
            Guarantee = 200,
            ArtistDoorPercent = 50
        };

        var result = (VersusContractDto)sut.ToDto(entity);

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.PaymentMethod, result.PaymentMethod);
        Assert.Equal(entity.Guarantee, result.Guarantee);
        Assert.Equal(entity.ArtistDoorPercent, result.ArtistDoorPercent);
    }
}
