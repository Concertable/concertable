using Application.DTOs;
using Application.Requests;
using Core.Entities;

namespace Application.Mappers
{
    public static class ReviewMappers
    {
        public static ReviewDto ToDto(this Review review) => new()
        {
            Id = review.Id,
            Stars = review.Stars,
            Details = review.Details,
            Email = review.Ticket.User.Email ?? string.Empty
        };

        public static Review ToEntity(this CreateReviewRequest request) => new()
        {
            Stars = request.Stars,
            Details = request.Details
        };

        public static IEnumerable<ReviewDto> ToDtos(this IEnumerable<Review> reviews) =>
            reviews.Select(r => r.ToDto());
    }
}
