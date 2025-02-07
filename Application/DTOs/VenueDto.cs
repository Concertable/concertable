namespace Application.DTOs
{
    public class VenueDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public CoordinatesDto Coordinates { get; set; }
        public string County { get; set; }
        public string Town { get; set; }
        public string ImageUrl { get; set; }
        public List<ListingDto>[]? Listings { get; set; } 
        public bool Approved { get; set; }
    }
}
