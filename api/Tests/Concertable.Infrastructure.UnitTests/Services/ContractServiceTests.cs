using Concertable.Application.DTOs;
using Concertable.Core.Enums;
using Concertable.Shared.Exceptions;
using Concertable.Concert.Infrastructure.Services;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services;

public class ContractServiceTests
{
    private readonly Mock<IContractRepository> contractRepository;
    private readonly Mock<IContractMapper> contractMapper;
    private readonly ContractService sut;

    public ContractServiceTests()
    {
        contractRepository = new Mock<IContractRepository>();
        contractMapper = new Mock<IContractMapper>();
        sut = new ContractService(contractRepository.Object, contractMapper.Object);
    }

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
}
