using Concertable.Application.Responses;

namespace Concertable.Web.Handlers;

public interface IConcertPostedHandler
{
    Task HandleAsync(ConcertPostResponse result);
}
