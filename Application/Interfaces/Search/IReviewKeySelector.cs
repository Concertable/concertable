using Core.Entities;
using System.Linq.Expressions;

namespace Application.Interfaces.Search;

public interface IReviewKeySelector<TEntity>
{
    Expression<Func<Review, int>> KeySelector { get; }
}
