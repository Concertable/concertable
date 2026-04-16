using Concertable.Application.DTOs;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Entities.Contracts;
using Concertable.Core.Enums;
using Concertable.Application.Exceptions;
using Concertable.Infrastructure.Services.Concert;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services;

public class ContractServiceTests
{
    private readonly Mock<IContractRepository> contractRepository;
    private readonly Mock<IContractMapper> contractMapper;
    private readonly Mock<IContractServiceStrategy> contractServiceStrategy;
    private readonly ContractService sut;

    public ContractServiceTests()
    {
        contractRepository = new Mock<IContractRepository>();
        contractMapper = new Mock<IContractMapper>();
        contractServiceStrategy = new Mock<IContractServiceStrategy>();
        sut = new ContractService(contractRepository.Object, contractMapper.Object, contractServiceStrategy.Object);
    }

    #region GetByOpportunityIdAsync

    [Fact]
    public async Task GetByOpportunityIdAsync_ShouldThrowNotFoundException_WhenContractNotFound()
    {
        contractRepository
            .Setup(r => r.GetByOpportunityIdAsync<ContractEntity>(1))
            .ReturnsAsync((ContractEntity?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => sut.GetByOpportunityIdAsync(1));
    }

    [Fact]
    public async Task GetByOpportunityIdAsync_ShouldReturnMappedDto()
    {
        var entity = FlatFeeContractEntity.Create(500, PaymentMethod.Cash);
        var expected = new FlatFeeContractDto { Id = 1, PaymentMethod = PaymentMethod.Cash, Fee = 500 };
        contractRepository
            .Setup(r => r.GetByOpportunityIdAsync<ContractEntity>(1))
            .ReturnsAsync(entity);
        contractMapper
            .Setup(m => m.ToDto(entity))
            .Returns(expected);

        var result = (FlatFeeContractDto)await sut.GetByOpportunityIdAsync(1);

        Assert.Equal(expected.Id, result.Id);
        Assert.Equal(expected.PaymentMethod, result.PaymentMethod);
        Assert.Equal(expected.Fee, result.Fee);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenContractNotFound()
    {
        contractRepository
            .Setup(r => r.GetByOpportunityIdAsync<ContractEntity>(1))
            .ReturnsAsync((ContractEntity?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.UpdateAsync(new FlatFeeContractDto { Id = 1 }, 1));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowBadRequestException_WhenContractTypeMismatches()
    {
        contractRepository
            .Setup(r => r.GetByOpportunityIdAsync<ContractEntity>(1))
            .ReturnsAsync(FlatFeeContractEntity.Create(1, PaymentMethod.Cash));

        await Assert.ThrowsAsync<BadRequestException>(() =>
            sut.UpdateAsync(new VersusContractDto { Id = 1 }, 1));
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallApplyChanges_WhenContractTypeMatches()
    {
        var existing = FlatFeeContractEntity.Create(500, PaymentMethod.Cash);
        var dto = new FlatFeeContractDto { Id = 1, PaymentMethod = PaymentMethod.Transfer, Fee = 750 };
        contractRepository
            .Setup(r => r.GetByOpportunityIdAsync<ContractEntity>(1))
            .ReturnsAsync(existing);

        await sut.UpdateAsync(dto, 1);

        contractServiceStrategy.Verify(s => s.ApplyChanges(existing, dto), Times.Once);
    }

    #endregion
}
