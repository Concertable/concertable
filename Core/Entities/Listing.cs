

namespace Core.Entities
{
    public class Listing : BaseEntity
    {
        public int VenueId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Pay {  get; set; }
        public Venue Venue { get; set; }
        public ICollection<ListingApplication> Registers { get; set; }
        public ICollection<ListingGenre> ListingGenres { get; set; }
    }
}
