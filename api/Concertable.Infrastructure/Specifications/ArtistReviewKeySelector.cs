using Concertable.Application.Interfaces.Search;
using Concertable.Core.Entities;
using System.Linq.Expressions;

namespace Concertable.Infrastructure.Specifications;

public class ArtistReviewKeySelector : IReviewKeySelector<ArtistEntity>
{
    public Expression<Func<ReviewEntity, int>> KeySelector => r => r.Ticket.Concert.Application.ArtistId;
}
