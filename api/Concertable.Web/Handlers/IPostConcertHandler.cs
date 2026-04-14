using Concertable.Application.Responses;

namespace Concertable.Web.Handlers;

public interface IPostConcertHandler
{
    Task HandleAsync(ConcertPostResponse result);
}
