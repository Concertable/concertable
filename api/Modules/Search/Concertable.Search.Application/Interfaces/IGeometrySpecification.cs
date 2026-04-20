namespace Concertable.Search.Application.Interfaces;

public interface IGeometrySpecification<TEntity> where TEntity : class, IIdEntity, ILocatable<TEntity>
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, IGeoParams geoParams);
}
