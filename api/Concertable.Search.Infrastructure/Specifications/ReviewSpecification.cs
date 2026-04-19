using Concertable.Application.Interfaces.Search;
using Concertable.Core.Entities;
using Concertable.Core.Interfaces;
using Concertable.Infrastructure.Expressions;
using System.Linq.Expressions;

namespace Concertable.Search.Infrastructure.Specifications;

internal class ReviewSpecification<TEntity> : IReviewSpecification<TEntity>
    where TEntity : class, IIdEntity, IReviewable<TEntity>
{
    public IQueryable<ReviewEntity> Apply(IQueryable<ReviewEntity> reviews, int id)
    {
        Expression<Func<int, bool>> condition = reviewId => reviewId == id;
        return reviews.Where(TEntity.ReviewIdSelector.Substitute(condition));
    }
}
