using Concertable.Application.Mappers;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Factories;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Factories;

public class ContractMapperFactoryTests
{
    private readonly ContractMapperFactory sut = new();

    [Fact]
    public void Create_ShouldReturnFlatFeeContractMapper_ForFlatFeeType()
    {
        var result = sut.Create(ContractType.FlatFee);

        Assert.IsType<FlatFeeContractMapper>(result);
    }

    [Fact]
    public void Create_ShouldReturnDoorSplitContractMapper_ForDoorSplitType()
    {
        var result = sut.Create(ContractType.DoorSplit);

        Assert.IsType<DoorSplitContractMapper>(result);
    }

    [Fact]
    public void Create_ShouldReturnVersusContractMapper_ForVersusType()
    {
        var result = sut.Create(ContractType.Versus);

        Assert.IsType<VersusContractMapper>(result);
    }

    [Fact]
    public void Create_ShouldReturnVenueHireContractMapper_ForVenueHireType()
    {
        var result = sut.Create(ContractType.VenueHire);

        Assert.IsType<VenueHireContractMapper>(result);
    }

    [Fact]
    public void Create_ShouldHandleAllContractTypes()
    {
        foreach (var type in Enum.GetValues<ContractType>())
            sut.Create(type);
    }
}
