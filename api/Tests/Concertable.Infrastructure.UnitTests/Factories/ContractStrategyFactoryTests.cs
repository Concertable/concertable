using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Enums;
using Concertable.Infrastructure.Factories;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Factories;

public class ContractStrategyFactoryTests
{
    private static readonly IConcertWorkflowStrategy flatFeeStrategy = new Mock<IConcertWorkflowStrategy>().Object;
    private static readonly IConcertWorkflowStrategy doorSplitStrategy = new Mock<IConcertWorkflowStrategy>().Object;
    private static readonly IConcertWorkflowStrategy versusStrategy = new Mock<IConcertWorkflowStrategy>().Object;
    private static readonly IConcertWorkflowStrategy venueHireStrategy = new Mock<IConcertWorkflowStrategy>().Object;
    private readonly ContractStrategyFactory<IConcertWorkflowStrategy> sut;

    public ContractStrategyFactoryTests()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IConcertWorkflowStrategy>(ContractType.FlatFee, flatFeeStrategy);
        services.AddKeyedSingleton<IConcertWorkflowStrategy>(ContractType.DoorSplit, doorSplitStrategy);
        services.AddKeyedSingleton<IConcertWorkflowStrategy>(ContractType.Versus, versusStrategy);
        services.AddKeyedSingleton<IConcertWorkflowStrategy>(ContractType.VenueHire, venueHireStrategy);

        sut = new ContractStrategyFactory<IConcertWorkflowStrategy>(services.BuildServiceProvider());
    }

    public static TheoryData<ContractType, IConcertWorkflowStrategy> ContractTypeStrategies => new()
    {
        { ContractType.FlatFee, flatFeeStrategy },
        { ContractType.DoorSplit, doorSplitStrategy },
        { ContractType.Versus, versusStrategy },
        { ContractType.VenueHire, venueHireStrategy }
    };

    [Theory]
    [MemberData(nameof(ContractTypeStrategies))]
    public void Create_ShouldReturnCorrectStrategy(ContractType contractType, IConcertWorkflowStrategy expected)
    {
        Assert.Same(expected, sut.Create(contractType));
    }
}
