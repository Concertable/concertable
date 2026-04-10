using Concertable.Application.DTOs;
using Concertable.Application.Mappers;
using Concertable.Core.Entities;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Factories;

public class TransactionMapperTests
{
    private readonly TransactionMapper sut = new();

    [Fact]
    public void ToEntity_WithTicketDto_ReturnsTicketTransactionEntity()
    {
        var dto = new TicketTransactionDto { FromUserId = Guid.NewGuid(), ToUserId = Guid.NewGuid(), PaymentIntentId = "pi_test" };

        var result = sut.ToEntity(dto);

        Assert.IsType<TicketTransactionEntity>(result);
    }

    [Fact]
    public void ToEntity_WithSettlementDto_ReturnsSettlementTransactionEntity()
    {
        var dto = new SettlementTransactionDto { FromUserId = Guid.NewGuid(), ToUserId = Guid.NewGuid(), PaymentIntentId = "pi_test" };

        var result = sut.ToEntity(dto);

        Assert.IsType<SettlementTransactionEntity>(result);
    }

    [Fact]
    public void ToDto_WithTicketEntity_ReturnsTicketTransactionDto()
    {
        var entity = new TicketTransactionEntity { PaymentIntentId = "pi_test", CreatedBy = "test" };

        var result = sut.ToDto(entity);

        Assert.IsType<TicketTransactionDto>(result);
    }

    [Fact]
    public void ToDto_WithSettlementEntity_ReturnsSettlementTransactionDto()
    {
        var entity = new SettlementTransactionEntity { PaymentIntentId = "pi_test", CreatedBy = "test" };

        var result = sut.ToDto(entity);

        Assert.IsType<SettlementTransactionDto>(result);
    }
}
