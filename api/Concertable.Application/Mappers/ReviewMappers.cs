using Concertable.Application.DTOs;
using Concertable.Core.Entities;

namespace Concertable.Application.Mappers;

public static class ReviewMappers
{
    public static ReviewDto ToDto(this ReviewEntity review) => new()
    {
        Id = review.Id,
        Stars = review.Stars,
        Details = review.Details,
        Email = review.Ticket.User.Email ?? string.Empty
    };

    public static IEnumerable<ReviewDto> ToDtos(this IEnumerable<ReviewEntity> reviews) =>
        reviews.Select(r => r.ToDto());
}
