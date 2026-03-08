namespace Application.Requests
{
    public record TransactionRequest
    {
        public required string PaymentMethodId { get; set; }
        public required string FromUserEmail { get; set; }
        public string? DestinationStripeId { get; set; }
        public decimal Amount { get; set; }
        public required Dictionary<string, string> Metadata { get; set; }
    }

    public record PaymentRequest
    {
    }
}
