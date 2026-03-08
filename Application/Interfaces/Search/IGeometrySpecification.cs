using Core.Interfaces;
using Core.Parameters;

namespace Application.Interfaces.Search
{
    public interface IGeometrySpecification<TEntity> where TEntity : IHasLocation
    {
        IQueryable<TEntity> Apply(IQueryable<TEntity> query, SearchParams searchParams);
    }
}
