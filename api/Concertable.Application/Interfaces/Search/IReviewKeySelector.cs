using Concertable.Core.Entities;
using System.Linq.Expressions;

namespace Concertable.Application.Interfaces.Search;

public interface IReviewKeySelector<TEntity>
{
    Expression<Func<ReviewEntity, int>> KeySelector { get; }
}
