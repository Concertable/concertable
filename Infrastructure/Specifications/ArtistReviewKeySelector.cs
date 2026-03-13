using Application.Interfaces.Search;
using Core.Entities;
using System.Linq.Expressions;

namespace Infrastructure.Specifications;

public class ArtistReviewKeySelector : IReviewKeySelector<Artist>
{
    public Expression<Func<Review, int>> KeySelector => r => r.Ticket.Concert.Application.ArtistId;
}
