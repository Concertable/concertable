using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class VenueDto : ItemDto
    {
        public int Id { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool Approved { get; set; } = false;

        public VenueDto()
        {
            Type = "venue";
        }
    }
}