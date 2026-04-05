using Concertable.Application.Interfaces.Search;
using Concertable.Core.Entities;
using System.Linq.Expressions;

namespace Concertable.Infrastructure.Expressions.Selectors;

public class VenueReviewKeySelector : IReviewKeySelector<VenueEntity>
{
    public Expression<Func<ReviewEntity, int>> KeySelector => r => r.Ticket.Concert.Application.Opportunity.VenueId;
}
