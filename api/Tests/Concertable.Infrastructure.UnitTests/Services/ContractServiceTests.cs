using Application.DTOs;
using Application.Interfaces.Concert;
using Application.Mappers;
using Concertable.Core.Entities.Contracts;
using Core.Enums;
using Core.Exceptions;
using Infrastructure.Services.Concert;
using Moq;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Services;

public class ContractServiceTests
{
    private readonly Mock<IContractRepository> contractRepository;
    private readonly Mock<IContractMapperFactory> mapperFactory;
    private readonly ContractService sut;

    public ContractServiceTests()
    {
        contractRepository = new Mock<IContractRepository>();
        mapperFactory = new Mock<IContractMapperFactory>();
        sut = new ContractService(contractRepository.Object, mapperFactory.Object);
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
        var entity = new FlatFeeContractEntity { Id = 1, PaymentMethod = PaymentMethod.Cash, Fee = 500 };
        var expected = new FlatFeeContractDto { Id = 1, PaymentMethod = PaymentMethod.Cash, Fee = 500 };
        contractRepository
            .Setup(r => r.GetByOpportunityIdAsync<ContractEntity>(1))
            .ReturnsAsync(entity);
        mapperFactory
            .Setup(f => f.Create(ContractType.FlatFee))
            .Returns(new FlatFeeContractMapper());

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
            .ReturnsAsync(new FlatFeeContractEntity { Id = 1 });

        await Assert.ThrowsAsync<BadRequestException>(() =>
            sut.UpdateAsync(new VersusContractDto { Id = 1 }, 1));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_ForFlatFee()
    {
        var existing = new FlatFeeContractEntity { Id = 1, PaymentMethod = PaymentMethod.Cash, Fee = 500 };
        contractRepository
            .Setup(r => r.GetByOpportunityIdAsync<ContractEntity>(1))
            .ReturnsAsync(existing);
        mapperFactory
            .Setup(f => f.Create(ContractType.FlatFee))
            .Returns(new FlatFeeContractMapper());

        await sut.UpdateAsync(new FlatFeeContractDto { Id = 1, PaymentMethod = PaymentMethod.Transfer, Fee = 750 }, 1);

        Assert.Equal(PaymentMethod.Transfer, existing.PaymentMethod);
        Assert.Equal(750, existing.Fee);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_ForDoorSplit()
    {
        var existing = new DoorSplitContractEntity { Id = 1, ArtistDoorPercent = 50 };
        contractRepository
            .Setup(r => r.GetByOpportunityIdAsync<ContractEntity>(1))
            .ReturnsAsync(existing);
        mapperFactory
            .Setup(f => f.Create(ContractType.DoorSplit))
            .Returns(new DoorSplitContractMapper());

        await sut.UpdateAsync(new DoorSplitContractDto { Id = 1, ArtistDoorPercent = 70 }, 1);

        Assert.Equal(70, existing.ArtistDoorPercent);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_ForVersus()
    {
        var existing = new VersusContractEntity { Id = 1, Guarantee = 100, ArtistDoorPercent = 50 };
        contractRepository
            .Setup(r => r.GetByOpportunityIdAsync<ContractEntity>(1))
            .ReturnsAsync(existing);
        mapperFactory
            .Setup(f => f.Create(ContractType.Versus))
            .Returns(new VersusContractMapper());

        await sut.UpdateAsync(new VersusContractDto { Id = 1, Guarantee = 200, ArtistDoorPercent = 60 }, 1);

        Assert.Equal(200, existing.Guarantee);
        Assert.Equal(60, existing.ArtistDoorPercent);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_ForVenueHire()
    {
        var existing = new VenueHireContractEntity { Id = 1, HireFee = 300 };
        contractRepository
            .Setup(r => r.GetByOpportunityIdAsync<ContractEntity>(1))
            .ReturnsAsync(existing);
        mapperFactory
            .Setup(f => f.Create(ContractType.VenueHire))
            .Returns(new VenueHireContractMapper());

        await sut.UpdateAsync(new VenueHireContractDto { Id = 1, HireFee = 500 }, 1);

        Assert.Equal(500, existing.HireFee);
    }

    #endregion
}
