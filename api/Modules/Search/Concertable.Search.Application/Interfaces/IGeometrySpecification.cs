namespace Concertable.Search.Application.Interfaces;

internal interface IGeometrySpecification<TEntity> where TEntity : class, IIdEntity, ILocatable<TEntity>
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query, IGeoParams geoParams);
}
