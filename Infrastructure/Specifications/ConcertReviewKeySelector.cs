using Application.Interfaces.Search;
using Core.Entities;
using System.Linq.Expressions;

namespace Infrastructure.Specifications;

public class ConcertReviewKeySelector : IReviewKeySelector<Concert>
{
    public Expression<Func<Review, int>> KeySelector => r => r.Ticket.ConcertId;
}
