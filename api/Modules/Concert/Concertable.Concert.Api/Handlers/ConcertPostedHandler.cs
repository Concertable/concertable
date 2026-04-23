using Concertable.Application.Interfaces;

namespace Concertable.Concert.Api.Handlers;

internal class ConcertPostedHandler : IConcertPostedHandler
{
    private readonly IBackgroundTaskRunner taskRunner;
    private readonly IConcertNotificationService notificationService;

    public ConcertPostedHandler(IBackgroundTaskRunner taskRunner, IConcertNotificationService notificationService)
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
