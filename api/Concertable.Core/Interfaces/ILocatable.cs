using Concertable.Core.Entities.Interfaces;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;

namespace Concertable.Core.Interfaces;

public interface ILocatable<TSelf> where TSelf : class, IEntity
{
    static abstract Expression<Func<TSelf, Point?>> LocationExpression { get; }
}
