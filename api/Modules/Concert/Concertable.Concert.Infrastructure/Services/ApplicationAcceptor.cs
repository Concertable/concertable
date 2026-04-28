using Concertable.Shared.Exceptions;

namespace Concertable.Concert.Infrastructure.Services;

internal class ApplicationAcceptor : IApplicationAcceptor
{
    private readonly IApplicationRepository applicationRepository;
    private readonly IBackgroundTaskRunner taskRunner;

    public ApplicationAcceptor(
        IApplicationRepository applicationRepository,
        IBackgroundTaskRunner taskRunner)
    {
        this.applicationRepository = applicationRepository;
        this.taskRunner = taskRunner;
    }

    public async Task AcceptAsync(int applicationId, BookingEntity booking)
    {
        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        application.Accept(booking);
        await applicationRepository.SaveChangesAsync();

        await taskRunner.RunAsync<IApplicationRepository>(
            (repo, ct) => repo.RejectAllExceptAsync(application.OpportunityId, applicationId));
    }
}
