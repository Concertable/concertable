using Concertable.Application.DTOs;
using Concertable.Core.Entities;

namespace Concertable.Infrastructure.Mappers;

public static class QueryableReviewMappers
{
    public static IQueryable<ReviewDto> ToDto(this IQueryable<ReviewEntity> query) =>
        query.Select(r => new ReviewDto
        {
            Id = r.Id,
            Stars = r.Stars,
            Details = r.Details,
            Email = string.Empty
        });

    public static IQueryable<ReviewSummaryDto> ToSummaryDto(this IQueryable<ReviewEntity> query) =>
        query
            .GroupBy(_ => true)
            .Select(g => new ReviewSummaryDto(
                g.Count(),
                g.Average(r => (double?)r.Stars)
            ));
}
