using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UpdateLocationRequest
    {
        [Required(ErrorMessage = "Latitude is required.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required.")]
        public double Longitude { get; set; }
    }
}
