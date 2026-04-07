using System;
using System.ComponentModel.DataAnnotations;

namespace Concertable.Core.Parameters;

public class TicketPurchaseParams
{
    public string? PaymentMethodId { get; set; }
    public int ConcertId { get; set; }
    public int Quantity { get; set; } = 1;
}
