using Application.Responses;
using Core.Entities;

namespace Application.Interfaces.Search;

public interface IRatingSpecification<TEntity>
{
    IQueryable<RatingResult> Apply(IQueryable<Review> reviews);
}
