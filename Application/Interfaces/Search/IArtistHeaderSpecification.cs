using Application.DTOs;
using Core.Entities;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IArtistHeaderSpecification
{
    IQueryable<ArtistHeaderDto> Apply(IQueryable<Artist> query, SearchParams searchParams);
}
