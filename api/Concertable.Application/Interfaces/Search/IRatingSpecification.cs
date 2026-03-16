using Core.Projections;
using Core.Entities;

namespace Application.Interfaces.Search;

public interface IRatingSpecification<TEntity>
{
    IQueryable<RatingAggregate> ApplyAggregate(IQueryable<ReviewEntity> reviews);
    IQueryable<double> ApplyAverage(IQueryable<ReviewEntity> reviews, int id);
}
