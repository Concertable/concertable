using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;
using Concertable.Seeding;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Mappers;

public class DoorSplitContractMapperTests
{
    private readonly DoorSplitContractMapper sut = new();

    [Fact]
    public void ToEntity_ShouldMapAllFields()
    {
        var dto = new DoorSplitContractDto { Id = 1, PaymentMethod = PaymentMethod.Cash, ArtistDoorPercent = 70 };

        var result = (DoorSplitContractEntity)sut.ToEntity(dto);

        Assert.Equal(dto.PaymentMethod, result.PaymentMethod);
        Assert.Equal(dto.ArtistDoorPercent, result.ArtistDoorPercent);
    }

    [Fact]
    public void ToDto_ShouldMapAllFields()
    {
        var entity = DoorSplitContractEntity.Create(70, PaymentMethod.Cash);

        var result = (DoorSplitContractDto)sut.ToDto(entity);

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.PaymentMethod, result.PaymentMethod);
        Assert.Equal(entity.ArtistDoorPercent, result.ArtistDoorPercent);
    }
}
