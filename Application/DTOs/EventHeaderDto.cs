namespace Application.DTOs
{
    public class EventHeaderDto : HeaderDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
