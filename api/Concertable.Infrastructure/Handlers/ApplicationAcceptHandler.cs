using Concertable.Application.Interfaces;
using Concertable.Application.Interfaces.Concert;
using Concertable.Core.Enums;
using Concertable.Core.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Concertable.Infrastructure.Handlers;

public class ApplicationAcceptHandler : IApplicationAcceptHandler
{
    private readonly IOpportunityApplicationRepository applicationRepository;
    private readonly IBackgroundTaskQueue taskQueue;
    private readonly IServiceScopeFactory scopeFactory;

    public ApplicationAcceptHandler(
        IOpportunityApplicationRepository applicationRepository,
        IBackgroundTaskQueue taskQueue,
        IServiceScopeFactory scopeFactory)
    {
        this.applicationRepository = applicationRepository;
        this.taskQueue = taskQueue;
        this.scopeFactory = scopeFactory;
    }

    public async Task HandleAsync(int applicationId)
    {
        var application = await applicationRepository.GetByIdAsync(applicationId)
            ?? throw new NotFoundException("Application not found");

        var opportunityId = application.OpportunityId;

        application.Status = ApplicationStatus.Accepted;
        await applicationRepository.SaveChangesAsync();

        await taskQueue.EnqueueAsync(async ct =>
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<IOpportunityApplicationRepository>();
            await repo.RejectAllExceptAsync(opportunityId, applicationId);
        });
    }
}
