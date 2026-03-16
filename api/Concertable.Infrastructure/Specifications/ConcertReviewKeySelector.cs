using Application.Interfaces.Search;
using Core.Entities;
using System.Linq.Expressions;

namespace Infrastructure.Specifications;

public class ConcertReviewKeySelector : IReviewKeySelector<ConcertEntity>
{
    public Expression<Func<ReviewEntity, int>> KeySelector => r => r.Ticket.ConcertId;
}
