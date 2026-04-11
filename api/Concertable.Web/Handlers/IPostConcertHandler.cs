using Concertable.Application.Results;

namespace Concertable.Web.Handlers;

public interface IPostConcertHandler
{
    Task HandleAsync(ConcertPostResult result);
}
