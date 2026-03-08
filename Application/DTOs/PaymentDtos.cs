namespace Application.DTOs
{
    public record PaymentDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "GBP";
        public required string PaymentMethodId { get; set; }
        public required string Description { get; set; }
        public int ConcertId { get; set; }
        public int UserId { get; set; }
    }

    public record TransactionDto
    {
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public required string TransactionId { get; set; }
        public long Amount { get; set; }
        public required string Type { get; set; }
        public required string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public record PurchaseCompleteDto
    {
        public int EntityId { get; set; }
        public required string TransactionId { get; set; }
        public int FromUserId { get; set; }
        public required string FromEmail { get; set; }
        public int ToUserId { get; set; }
        public int? Quantity { get; set; }
    }
}
