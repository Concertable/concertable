using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreateVenueDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(1, ErrorMessage = "Name cannot be empty.")] 
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "About section cannot exceed 1000 characters.")]
        public string About { get; set; }

        [Required(ErrorMessage = "Coordinates are required.")]
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [Required(ErrorMessage = "Image URL is required.")] 
        public string ImageUrl { get; set; }
    }
}
