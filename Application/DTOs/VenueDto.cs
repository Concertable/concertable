using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class VenueDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "About section cannot exceed 1000 characters.")]
        public string About { get; set; }

        [Required(ErrorMessage = "Coordinates are required.")]
        public CoordinatesDto Coordinates { get; set; }

        [Required(ErrorMessage = "County is required.")]
        [StringLength(50, ErrorMessage = "County cannot exceed 50 characters.")]
        public string County { get; set; }

        [Required(ErrorMessage = "Town is required.")]
        [StringLength(50, ErrorMessage = "Town cannot exceed 50 characters.")]
        public string Town { get; set; }

        [Required(ErrorMessage = "ImageUrl is required.")]
        public string ImageUrl { get; set; }

        public bool Approved { get; set; } = false; 
    }
}