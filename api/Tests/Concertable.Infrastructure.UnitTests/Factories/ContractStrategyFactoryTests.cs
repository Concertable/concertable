using Application.Interfaces.Concert;
using Core.Enums;
using Infrastructure.Factories;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Factories;

public class ContractStrategyFactoryTests
{
    private static readonly IApplicationStrategy flatFeeStrategy = new Mock<IApplicationStrategy>().Object;
    private static readonly IApplicationStrategy doorSplitStrategy = new Mock<IApplicationStrategy>().Object;
    private static readonly IApplicationStrategy versusStrategy = new Mock<IApplicationStrategy>().Object;
    private static readonly IApplicationStrategy venueHireStrategy = new Mock<IApplicationStrategy>().Object;
    private readonly ContractStrategyFactory<IApplicationStrategy> sut;

    public ContractStrategyFactoryTests()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IApplicationStrategy>(ContractType.FlatFee, flatFeeStrategy);
        services.AddKeyedSingleton<IApplicationStrategy>(ContractType.DoorSplit, doorSplitStrategy);
        services.AddKeyedSingleton<IApplicationStrategy>(ContractType.Versus, versusStrategy);
        services.AddKeyedSingleton<IApplicationStrategy>(ContractType.VenueHire, venueHireStrategy);

        sut = new ContractStrategyFactory<IApplicationStrategy>(services.BuildServiceProvider());
    }

    public static TheoryData<ContractType, IApplicationStrategy> ContractTypeStrategies => new()
    {
        { ContractType.FlatFee, flatFeeStrategy },
        { ContractType.DoorSplit, doorSplitStrategy },
        { ContractType.Versus, versusStrategy },
        { ContractType.VenueHire, venueHireStrategy }
    };

    [Theory]
    [MemberData(nameof(ContractTypeStrategies))]
    public void Create_ShouldReturnCorrectStrategy(ContractType contractType, IApplicationStrategy expected)
    {
        Assert.Same(expected, sut.Create(contractType));
    }
}
