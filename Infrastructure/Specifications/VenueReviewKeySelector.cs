using Application.Interfaces.Search;
using Core.Entities;
using System.Linq.Expressions;

namespace Infrastructure.Specifications;

public class VenueReviewKeySelector : IReviewKeySelector<Venue>
{
    public Expression<Func<Review, int>> KeySelector => r => r.Ticket.Concert.Application.Opportunity.VenueId;
}
