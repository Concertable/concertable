using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class VenueDto : ItemDto
    {
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
        public double Latitude { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "An image URL is required.")]
        [Url(ErrorMessage = "ImageUrl must be a valid URL.")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "County is required.")]
        [StringLength(100, ErrorMessage = "County must be under 100 characters.")]
        public string County { get; set; }

        [Required(ErrorMessage = "Town is required.")]
        [StringLength(100, ErrorMessage = "Town must be under 100 characters.")]
        public string Town { get; set; }

        public bool Approved { get; set; } = false;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        public VenueDto()
        {
            Type = "venue";
        }
    }
}
