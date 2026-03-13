using Application.DTOs;
using Core.Entities;
using Core.Parameters;

namespace Application.Interfaces.Search;

public interface IConcertHeaderSpecification
{
    IQueryable<ConcertHeaderDto> Apply(IQueryable<Concert> query, SearchParams searchParams);
}
