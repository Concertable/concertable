using Concertable.Application.Responses;

namespace Concertable.Web.Handlers;

internal interface IConcertPostedHandler
{
    Task HandleAsync(ConcertPostResponse result);
}
