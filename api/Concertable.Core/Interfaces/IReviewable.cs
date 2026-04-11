using Concertable.Core.Entities;
using Concertable.Core.Entities.Interfaces;
using System.Linq.Expressions;

namespace Concertable.Core.Interfaces;

public interface IReviewable<TSelf> where TSelf : class, IIdEntity
{
    static abstract Expression<Func<ReviewEntity, int>> ReviewIdSelector { get; }
}
