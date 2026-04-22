namespace Concertable.Concert.Application.Requests;

internal class ConcertBookingParams
{
    public required string PaymentMethodId { get; set; }
    public int ApplicationId { get; set; }
}
