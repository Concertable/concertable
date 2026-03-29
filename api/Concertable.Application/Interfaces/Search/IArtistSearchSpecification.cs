using Concertable.Core.Entities;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Search;

public interface IArtistSearchSpecification
{
    IQueryable<ArtistEntity> Apply(IQueryable<ArtistEntity> query, SearchParams searchParams);
}
