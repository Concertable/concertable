using Concertable.Core.Projections;

namespace Concertable.Search.Application.Interfaces;

public interface IRatingSpecification<T> where T : class, IIdEntity
{
    IQueryable<RatingAggregate> ApplyAggregate(IQueryable<ReviewEntity> reviews);
    IQueryable<double?> ApplyAverage(IQueryable<ReviewEntity> reviews, int id);
}
