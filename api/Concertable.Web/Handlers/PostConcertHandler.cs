using Concertable.Application.Interfaces;
using Concertable.Application.Results;

namespace Concertable.Web.Handlers;

public class PostConcertHandler : IPostConcertHandler
{
    private readonly IBackgroundTaskQueue queue;
    private readonly IConcertNotificationService notificationService;

    public PostConcertHandler(IBackgroundTaskQueue queue, IConcertNotificationService notificationService)
    {
        this.queue = queue;
        this.notificationService = notificationService;
    }

    public async Task HandleAsync(ConcertPostResult result)
    {
        await queue.EnqueueAsync(async ct =>
        {
            var tasks = result.UserIds.Select(userId =>
                notificationService.ConcertPostedAsync(userId.ToString(), result.ConcertHeader));

            await Task.WhenAll(tasks);
        });
    }
}
