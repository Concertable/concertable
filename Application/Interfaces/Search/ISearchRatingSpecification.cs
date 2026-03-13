using Core.Entities;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface ISearchRatingSpecification<TEntity> where TEntity : BaseEntity
{
    IQueryable<(TEntity Entity, double Rating)> Apply(IQueryable<TEntity> query);
}
