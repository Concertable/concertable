using Concertable.Core.Parameters;

namespace Concertable.Search.Application.Interfaces;

internal interface IArtistSearchSpecification
{
    IQueryable<ArtistEntity> Apply(IQueryable<ArtistEntity> query, SearchParams searchParams);
}
