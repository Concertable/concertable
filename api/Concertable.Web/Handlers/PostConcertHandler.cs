using Concertable.Application.Interfaces;
using Concertable.Application.Responses;

namespace Concertable.Web.Handlers;

public class PostConcertHandler : IPostConcertHandler
{
    private readonly IBackgroundTaskRunner taskRunner;
    private readonly IConcertNotificationService notificationService;

    public PostConcertHandler(IBackgroundTaskRunner taskRunner, IConcertNotificationService notificationService)
    {
        this.taskRunner = taskRunner;
        this.notificationService = notificationService;
    }

    public async Task HandleAsync(ConcertPostResponse result)
    {
        await taskRunner.RunAsync(async ct =>
        {
            var tasks = result.UserIds.Select(userId =>
                notificationService.ConcertPostedAsync(userId.ToString(), result.ConcertHeader));

            await Task.WhenAll(tasks);
        });
    }
}
