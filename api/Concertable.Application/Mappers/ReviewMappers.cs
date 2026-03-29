using Concertable.Application.DTOs;
using Concertable.Application.Requests;
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

    public static ReviewEntity ToEntity(this CreateReviewRequest request) => new()
    {
        Stars = request.Stars,
        Details = request.Details
    };

    public static IEnumerable<ReviewDto> ToDtos(this IEnumerable<ReviewEntity> reviews) =>
        reviews.Select(r => r.ToDto());
}
