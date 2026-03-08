namespace Application.Responses
{
    public record TicketPurchaseResponse : PurchaseResponse
    {
        public IEnumerable<int> TicketIds { get; set; } = [];
        public int ConcertId { get; set; }
    }
}
