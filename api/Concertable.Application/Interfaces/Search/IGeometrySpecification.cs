using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Search;

public interface IGeometrySpecification<TEntity> where TEntity : IHasLocation
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, IGeoParams searchParams);
}
