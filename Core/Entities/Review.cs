

namespace Core.Entities
{
    public class Review : BaseEntity
    {
        public int TicketId { get; set; }
        public int Stars { get; set; }
        public string? Details { get; set; }
        public Ticket Ticket { get; set; }
    }
}
