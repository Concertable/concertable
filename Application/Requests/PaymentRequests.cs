namespace Application.Requests
{
    public record TransactionRequest
    {
        public string PaymentMethodId { get; set; }
        public string FromUserEmail { get; set; }
        public string? DestinationStripeId { get; set; }
        public decimal Amount { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public record PaymentRequest
    {
    }
}
