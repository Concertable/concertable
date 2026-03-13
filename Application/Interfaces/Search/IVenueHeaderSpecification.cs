using Application.DTOs;
using Core.Entities;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IVenueHeaderSpecification
{
    IQueryable<VenueHeaderDto> Apply(IQueryable<Venue> query, SearchParams searchParams);
}
