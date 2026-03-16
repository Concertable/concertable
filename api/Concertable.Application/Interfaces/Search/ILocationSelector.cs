using Core.Interfaces;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Application.Interfaces.Search;

public interface ILocationSelector<TEntity> where TEntity : class, IHasLocation
{
    Expression<Func<TEntity, Point?>> LocationSelector { get; }
}
