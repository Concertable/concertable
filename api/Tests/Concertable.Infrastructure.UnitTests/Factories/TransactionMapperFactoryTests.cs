using Application.Mappers;
using Core.Enums;
using Infrastructure.Factories;
using Xunit;

namespace Concertable.Infrastructure.UnitTests.Factories;

public class TransactionMapperFactoryTests
{
    private readonly TransactionMapperFactory sut = new();

    [Fact]
    public void Create_ShouldReturnTicketTransactionMapper_ForTicketType()
    {
        var result = sut.Create(TransactionType.Ticket);

        Assert.IsType<TicketTransactionMapper>(result);
    }

    [Fact]
    public void Create_ShouldReturnSettlementTransactionMapper_ForSettlementType()
    {
        var result = sut.Create(TransactionType.Settlement);

        Assert.IsType<SettlementTransactionMapper>(result);
    }

    [Fact]
    public void Create_ShouldHandleAllTransactionTypes()
    {
        foreach (var type in Enum.GetValues<TransactionType>())
            sut.Create(type);
    }
}
