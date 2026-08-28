using Concertable.B2B.Concert.Application.DTOs;
using Concertable.B2B.Concert.Application.Responses;
using Concertable.B2B.Concert.Domain.Lifecycle;

namespace Concertable.B2B.Concert.Application.Mappers;

/// <summary>
/// DoorSplit and Guarantee Plus: the payee gross depends on the venue-declared door take. Carries the
/// formula, whether a declaration is still open, and the declared figures once the venue has entered them.
/// </summary>
internal sealed class RevenueShareSettlementMapper : ISettlementMapper
{
    private readonly IPaymentAmountMapper paymentAmountMapper;

    public RevenueShareSettlementMapper(IPaymentAmountMapper paymentAmountMapper)
    {
        this.paymentAmountMapper = paymentAmountMapper;
    }

    public ISettlement ToSettlement(DealDto deal, ManagerConcertDetailsProjection projection, DateTime nowUtc)
    {
        var concert = projection.Concert;
        var row = projection.Settlement;
        var ticketSalesMinor = Money.Gbp(concert.TicketsSold * concert.Price).ToMinorUnits();

        ISettlementDeclaration declaration = row switch
        {
            null => new Undeclared(WindowOpen: concert.State == LifecycleState.Booked && concert.EndDate < nowUtc),
            { Review: null } => new Declared(row.DoorRevenue, ticketSalesMinor, row.DeclaredAtUtc),
            { Review: { } review } => new Reviewed(
                row.DoorRevenue, ticketSalesMinor, row.DeclaredAtUtc, review.GrossMinor, review.ReviewedAtUtc),
        };

        return new RevenueShareSettlement(paymentAmountMapper.ToPaymentAmount(deal), declaration);
    }
}
