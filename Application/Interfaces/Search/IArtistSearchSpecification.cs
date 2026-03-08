using Core.Entities;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IArtistSearchSpecification
{
    IQueryable<Artist> Apply(IQueryable<Artist> query, SearchParams searchParams);
}
