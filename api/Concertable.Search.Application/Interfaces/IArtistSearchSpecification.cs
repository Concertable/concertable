using Concertable.Core.Entities;
using Concertable.Core.Parameters;

namespace Concertable.Search.Application.Interfaces;

public interface IArtistSearchSpecification
{
    IQueryable<ArtistEntity> Apply(IQueryable<ArtistEntity> query, SearchParams searchParams);
}
