using Concertable.Application.Interfaces;
using Concertable.Shared.Exceptions;

namespace Concertable.Infrastructure.Handlers;

internal class ApplicationAcceptHandler : IApplicationAcceptHandler
{
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IBackgroundTaskRunner taskRunner;

    public ApplicationAcceptHandler(
        IOpportunityApplicationRepository applicationRepository,
        IBackgroundTaskRunner taskRunner)
    {
        this.applicationRepository = applicationRepository;
        this.taskRunner = taskRunner;
    }

    public async Task HandleAsync(int applicationId, ConcertBookingEntity bookingConcert)
    {
        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        application.Accept(bookingConcert);
        await applicationRepository.SaveChangesAsync();

        await taskRunner.RunAsync<IOpportunityApplicationRepository>(
            (repo, ct) => repo.RejectAllExceptAsync(application.OpportunityId, applicationId));
    }
}
