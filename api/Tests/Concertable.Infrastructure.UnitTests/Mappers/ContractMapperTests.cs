using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Mappers;

public class ContractMapperTests
{
    private readonly ContractMapper sut = new();

    #region ToDto

    [Fact]
    public void ToDto_ShouldReturnFlatFeeContractDto_WhenEntityIsFlatFee()
    {
        var entity = FlatFeeContractEntity.Create(500, PaymentMethod.Cash);

        var result = sut.ToDto(entity);

        Assert.IsType<FlatFeeContractDto>(result);
    }

    [Fact]
    public void ToDto_ShouldReturnDoorSplitContractDto_WhenEntityIsDoorSplit()
    {
        var entity = DoorSplitContractEntity.Create(50, PaymentMethod.Cash);

        var result = sut.ToDto(entity);

        Assert.IsType<DoorSplitContractDto>(result);
    }

    [Fact]
    public void ToDto_ShouldReturnVersusContractDto_WhenEntityIsVersus()
    {
        var entity = VersusContractEntity.Create(0, 50, PaymentMethod.Cash);

        var result = sut.ToDto(entity);

        Assert.IsType<VersusContractDto>(result);
    }

    [Fact]
    public void ToDto_ShouldReturnVenueHireContractDto_WhenEntityIsVenueHire()
    {
        var entity = VenueHireContractEntity.Create(100, PaymentMethod.Cash);

        var result = sut.ToDto(entity);

        Assert.IsType<VenueHireContractDto>(result);
    }

    #endregion

    #region ToEntity

    [Fact]
    public void ToEntity_ShouldReturnFlatFeeContractEntity_WhenDtoIsFlatFee()
    {
        var dto = new FlatFeeContractDto { Id = 1, PaymentMethod = PaymentMethod.Cash, Fee = 500 };

        var result = sut.ToEntity(dto);

        Assert.IsType<FlatFeeContractEntity>(result);
    }

    [Fact]
    public void ToEntity_ShouldReturnDoorSplitContractEntity_WhenDtoIsDoorSplit()
    {
        var dto = new DoorSplitContractDto { Id = 1, PaymentMethod = PaymentMethod.Cash };

        var result = sut.ToEntity(dto);

        Assert.IsType<DoorSplitContractEntity>(result);
    }

    [Fact]
    public void ToEntity_ShouldReturnVersusContractEntity_WhenDtoIsVersus()
    {
        var dto = new VersusContractDto { Id = 1, PaymentMethod = PaymentMethod.Cash };

        var result = sut.ToEntity(dto);

        Assert.IsType<VersusContractEntity>(result);
    }

    [Fact]
    public void ToEntity_ShouldReturnVenueHireContractEntity_WhenDtoIsVenueHire()
    {
        var dto = new VenueHireContractDto { Id = 1, PaymentMethod = PaymentMethod.Cash, HireFee = 100 };

        var result = sut.ToEntity(dto);

        Assert.IsType<VenueHireContractEntity>(result);
    }

    #endregion
}
