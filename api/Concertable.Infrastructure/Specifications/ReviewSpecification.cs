using Concertable.Application.Interfaces.Search;
using System.Linq.Expressions;
using Concertable.Core.Entities;
using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Interfaces;
using Concertable.Infrastructure.Expressions;

namespace Concertable.Infrastructure.Specifications;

public class ReviewSpecification<TEntity> : IReviewSpecification<TEntity>
    where TEntity : class, IIdEntity, IReviewable<TEntity>
{
    public IQueryable<ReviewEntity> Apply(IQueryable<ReviewEntity> reviews, int id)
    {
        Expression<Func<int, bool>> condition = reviewId => reviewId == id;
        return reviews.Where(TEntity.ReviewIdSelector.Substitute(condition));
    }
}
