using Concertable.Core.Entities;
using Concertable.Core.Entities.Interfaces;
using Concertable.Core.Interfaces;

namespace Concertable.Application.Interfaces.Search;

public interface IReviewSpecification<TEntity> where TEntity : class, IIdEntity, IReviewable<TEntity>
{
    IQueryable<ReviewEntity> Apply(IQueryable<ReviewEntity> reviews, int id);
}
