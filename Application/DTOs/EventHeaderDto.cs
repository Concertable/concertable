namespace Application.DTOs
{
    public class EventHeaderDto : HeaderDto
    {
        public EventHeaderDto()
        {
            Type = "event";
        }
    }
}
