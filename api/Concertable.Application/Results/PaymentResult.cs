namespace Concertable.Application.Results;

public record PaymentResult
{
    public bool Success { get; set; }
    public bool RequiresAction { get; set; }
    public required string Message { get; set; }
    public string? ClientSecret { get; set; }
    public string? TransactionId { get; set; }
}
