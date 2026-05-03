
namespace Concertable.Concert.IntegrationTests.Ticket;

public static class TicketRequestBuilders
{
    public static TicketPurchaseParams BuildPurchaseRequest(
        int concertId = 1,
        string paymentMethodId = "pm_card_visa",
        int quantity = 1) => new()
    {
        ConcertId = concertId,
        PaymentMethodId = paymentMethodId,
        Quantity = quantity
    };
}
