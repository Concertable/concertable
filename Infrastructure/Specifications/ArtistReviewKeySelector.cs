using Application.Interfaces.Search;
using Core.Entities;
using System.Linq.Expressions;

namespace Infrastructure.Specifications;

public class ArtistReviewKeySelector : IReviewKeySelector<ArtistEntity>
{
    public Expression<Func<ReviewEntity, int>> KeySelector => r => r.Ticket.Concert.Application.ArtistId;
}
