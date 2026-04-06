using Concertable.Application.Interfaces.Search;
using Concertable.Core.Interfaces;

namespace Concertable.Infrastructure.Specifications;

public class HeaderSortSpecification<T> : ISortSpecification<T>
    where T : IHeader
{
    public IQueryable<T> Apply(IQueryable<T> query, ISortParams sortParams) =>
        sortParams.Sort?.ToLower() switch
        {
            "name_asc" => query.OrderBy(h => h.Name),
            "name_desc" => query.OrderByDescending(h => h.Name),
            _ => query.OrderBy(h => h.Id)
        };
}
