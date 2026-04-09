using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Interfaces;
using Concertable.Core.Parameters;

namespace Concertable.Application.Interfaces.Search;

public interface IGeometrySpecification<TEntity> where TEntity : class, IEntity, ILocatable<TEntity>
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, IGeoParams searchParams);
}
