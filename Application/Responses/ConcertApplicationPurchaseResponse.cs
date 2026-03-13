using Application.DTOs;

namespace Application.Responses;

public record ConcertApplicationPurchaseResponse : PurchaseResponse
{
    public int ApplicationId { get; set; }
    public ConcertDto? Concert { get; set; }
}
