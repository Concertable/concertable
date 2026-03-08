namespace Application.Responses
{
    public record PurchaseResponse
    {
        public bool Success { get; set; }
        public bool RequiresAction { get; set; }
        public required string Message { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? TransactionId { get; set; }
        public string? ClientSecret { get; set; }
        public string? UserEmail { get; set; }
    }
}
