using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class VenueDto : ItemDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ImageUrl { get; set; }
        public string County { get; set; }
        public string Town { get; set; }
        public bool Approved { get; set; } = false;
        public string Email { get; set; }

        public VenueDto()
        {
            Type = "venue";
        }
    }
}