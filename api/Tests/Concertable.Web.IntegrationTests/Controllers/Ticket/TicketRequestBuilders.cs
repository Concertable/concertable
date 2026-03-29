using Concertable.Core.Parameters;

namespace Concertable.Web.IntegrationTests.Controllers.Ticket;

public static class TicketRequestBuilders
{
    public static TicketPurchaseParams BuildPurchaseRequest(
        int concertId = 1,
        string paymentMethodId = "pm_test",
        int quantity = 1) => new()
    {
        ConcertId = concertId,
        PaymentMethodId = paymentMethodId,
        Quantity = quantity
    };
}
