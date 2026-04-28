using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Handlers;

internal class ApplicationAcceptHandler : IApplicationAcceptHandler
{
    private readonly IApplicationRepository applicationRepository;
    private readonly IBackgroundTaskRunner taskRunner;

    public ApplicationAcceptHandler(
        IApplicationRepository applicationRepository,
        IBackgroundTaskRunner taskRunner)
    {
        this.applicationRepository = applicationRepository;
        this.taskRunner = taskRunner;
    }

    public async Task HandleAsync(int applicationId, BookingEntity bookingConcert)
    {
        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        application.Accept(bookingConcert);
        await applicationRepository.SaveChangesAsync();

        await taskRunner.RunAsync<IApplicationRepository>(
            (repo, ct) => repo.RejectAllExceptAsync(application.OpportunityId, applicationId));
    }
}
