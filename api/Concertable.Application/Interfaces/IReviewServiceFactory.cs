using Concertable.Core.Enums;

namespace Concertable.Application.Interfaces;

public interface IReviewServiceFactory
{
    IReviewService Create(ReviewType type);
}
