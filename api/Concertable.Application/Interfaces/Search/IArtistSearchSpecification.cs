using Core.Entities;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IArtistSearchSpecification
{
    IQueryable<ArtistEntity> Apply(IQueryable<ArtistEntity> query, SearchParams searchParams);
}
