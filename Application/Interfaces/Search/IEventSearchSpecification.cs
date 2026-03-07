using Core.Entities;
using Core.Parameters;

namespace Application.Interfaces.Search
{
    public interface IEventSearchSpecification
    {
        IQueryable<Event> Apply(IQueryable<Event> query, SearchParams searchParams);
    }
}
