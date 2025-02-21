using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class VenueDto : ItemDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Coordinates are required.")]
        public CoordinatesDto Coordinates { get; set; }

        [Required(ErrorMessage = "County is required.")]
        [StringLength(50, ErrorMessage = "County cannot exceed 50 characters.")]
        public string County { get; set; }

        [Required(ErrorMessage = "Town is required.")]
        [StringLength(50, ErrorMessage = "Town cannot exceed 50 characters.")]
        public string Town { get; set; }
        public bool Approved { get; set; } = false;

        public VenueDto()
        {
            Type = "venue";
        }
    }
}