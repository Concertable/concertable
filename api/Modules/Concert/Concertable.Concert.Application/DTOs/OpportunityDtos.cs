namespace Concertable.Concert.Application.DTOs;

internal record OpportunityDto
{
    public int? Id { get; set; }
    public int VenueId { get; set; }
    public int ContractId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public IEnumerable<GenreDto> Genres { get; set; } = [];
}
