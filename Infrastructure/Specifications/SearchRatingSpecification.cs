using Application.Interfaces.Search;
using Core.Entities;
using Infrastructure.Data.Identity;

namespace Infrastructure.Specifications;

public class SearchRatingSpecification<TEntity> : ISearchRatingSpecification<TEntity>
    where TEntity : BaseEntity
{
    private readonly IRatingSpecification<TEntity> ratingSpecification;
    private readonly ApplicationDbContext context;

    public SearchRatingSpecification(IRatingSpecification<TEntity> ratingSpecification, ApplicationDbContext context)
    {
        this.ratingSpecification = ratingSpecification;
        this.context = context;
    }

    public IQueryable<(TEntity Entity, double Rating)> Apply(IQueryable<TEntity> query)
    {
        var ratings = ratingSpecification.Apply(context.Reviews);

        return from e in query
               join r in ratings on e.Id equals r.EntityId into rg
               from rating in rg.DefaultIfEmpty()
               select (e, rating != null ? rating.AverageRating : 0.0);
    }
}
