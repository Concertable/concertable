using Application.Responses;
using Core.Parameters;

namespace Application.Interfaces.Search
{
    public interface ISearchService
    {
        Task<Pagination<ISearchHeader>> SearchAsync(SearchParams searchParams);
    }
}
