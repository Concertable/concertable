using Core.Projections;
using Core.Entities;

namespace Application.Interfaces.Search;

public interface IRatingSpecification<TEntity>
{
    IQueryable<RatingAggregate> ApplyAggregate(IQueryable<Review> reviews);
    IQueryable<double> ApplyAverage(IQueryable<Review> reviews, int id);
}
