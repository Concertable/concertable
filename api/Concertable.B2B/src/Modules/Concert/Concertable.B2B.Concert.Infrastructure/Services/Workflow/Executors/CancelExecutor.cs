using Concertable.B2B.Concert.Application.Workflow;
using Concertable.B2B.Concert.Application.Workflow.Executors;
using Concertable.B2B.Concert.Domain.Lifecycle;
using Concertable.B2B.Concert.Infrastructure;
using Concertable.Kernel.Exceptions;
using Concertable.B2B.Tenant.Contracts;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Concertable.B2B.Concert.Infrastructure.Services.Workflow.Executors;

internal sealed class CancelExecutor : ICancelExecutor
{
    private readonly ILifecycleTransitioner transitioner;
    private readonly IConcertWorkflowFactory workflows;
    private readonly IDealResolver dealResolver;
    private readonly IConcertRepository concertRepository;
    private readonly ITenantModule tenantModule;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<CancelExecutor> logger;

    public CancelExecutor(
        ILifecycleTransitioner transitioner,
        IConcertWorkflowFactory workflows,
        IDealResolver dealResolver,
        IConcertRepository concertRepository,
        ITenantModule tenantModule,
        TimeProvider timeProvider,
        ILogger<CancelExecutor> logger)
    {
        this.transitioner = transitioner;
        this.workflows = workflows;
        this.dealResolver = dealResolver;
        this.concertRepository = concertRepository;
        this.tenantModule = tenantModule;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public async Task<Result> CancelAsync(int concertId, CancellationToken ct = default)
    {
        try
        {
            var concert = await concertRepository.GetByIdWithBookingAsync(concertId, ct)
                .OrNotFound();
            var configuration = await tenantModule.GetConfigurationAsync(concert.VenueTenantId, ct);
            if (timeProvider.GetUtcNow().UtcDateTime > concert.Period.Start.AddHours(-configuration.CancellationNoticeHours))
                throw new BadRequestException("The cancellation notice deadline has passed");

            await transitioner.TransitionAsync(concert.Booking.ApplicationId, Trigger.Cancel, async app =>
            {
                await dealResolver.ResolveByConcertIdAsync(concertId);
                var workflow = workflows.Create(app.DealType);
                await workflow.Cancel.ExecuteAsync(concertId);
                concert.Cancel();
            }, ct);
            return Result.Ok();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.FailedToCancelConcert(concertId, ex);
            return Result.Fail(ex.Message);
        }
    }
}
