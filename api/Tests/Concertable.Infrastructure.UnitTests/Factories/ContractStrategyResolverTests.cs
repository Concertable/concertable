using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;
using Concertable.Infrastructure.Factories;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Factories;

public class ContractStrategyResolverTests
{
    private readonly Mock<IContractRepository> contractRepository;
    private readonly Mock<IContractStrategyFactory<IConcertWorkflowStrategy>> factory;
    private readonly ContractStrategyResolver<IConcertWorkflowStrategy> sut;

    public ContractStrategyResolverTests()
    {
        contractRepository = new Mock<IContractRepository>();
        factory = new Mock<IContractStrategyFactory<IConcertWorkflowStrategy>>();
        sut = new ContractStrategyResolver<IConcertWorkflowStrategy>(contractRepository.Object, factory.Object);
    }

    #region ResolveForApplicationAsync

    [Fact]
    public async Task ResolveForApplicationAsync_ShouldReturnCorrectStrategy()
    {
        var strategy = new Mock<IConcertWorkflowStrategy>().Object;
        contractRepository.Setup(x => x.GetTypeByApplicationIdAsync(1)).ReturnsAsync(ContractType.FlatFee);
        factory.Setup(x => x.Create(ContractType.FlatFee)).Returns(strategy);

        var result = await sut.ResolveForApplicationAsync(1);

        Assert.Same(strategy, result);
    }

    [Fact]
    public async Task ResolveForApplicationAsync_ShouldThrowNotFoundException_WhenContractNotFound()
    {
        contractRepository.Setup(x => x.GetTypeByApplicationIdAsync(1)).ReturnsAsync((ContractType?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.ResolveForApplicationAsync(1));
    }

    #endregion

    #region ResolveForConcertAsync

    [Fact]
    public async Task ResolveForConcertAsync_ShouldReturnCorrectStrategy()
    {
        var strategy = new Mock<IConcertWorkflowStrategy>().Object;
        contractRepository.Setup(x => x.GetTypeByConcertIdAsync(1)).ReturnsAsync(ContractType.DoorSplit);
        factory.Setup(x => x.Create(ContractType.DoorSplit)).Returns(strategy);

        var result = await sut.ResolveForConcertAsync(1);

        Assert.Same(strategy, result);
    }

    [Fact]
    public async Task ResolveForConcertAsync_ShouldThrowNotFoundException_WhenContractNotFound()
    {
        contractRepository.Setup(x => x.GetTypeByConcertIdAsync(1)).ReturnsAsync((ContractType?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.ResolveForConcertAsync(1));
    }

    #endregion
}
