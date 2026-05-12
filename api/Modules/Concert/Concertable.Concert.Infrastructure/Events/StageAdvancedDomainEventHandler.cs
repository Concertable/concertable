using Concertable.Concert.Contracts.Events;
using Concertable.Concert.Domain.Events;
using Concertable.Shared;

namespace Concertable.Concert.Infrastructure.Events;

internal class StageAdvancedDomainEventHandler : IDomainEventHandler<StageAdvancedDomainEvent>
{
    private readonly IConcertLifecycleRepository lifecycleRepository;
    private readonly IIntegrationEventBus bus;

    public StageAdvancedDomainEventHandler(
        IConcertLifecycleRepository lifecycleRepository,
        IIntegrationEventBus bus)
    {
        this.lifecycleRepository = lifecycleRepository;
        this.bus = bus;
    }

    public async Task HandleAsync(StageAdvancedDomainEvent e, CancellationToken ct = default)
    {
        switch (e.To)
        {
            case ConcertStage.Applied:
                await PublishAppliedAsync(e.LifecycleId, ct);
                break;
            case ConcertStage.Accepted:
                await PublishAcceptedAsync(e.LifecycleId, ct);
                break;
            case ConcertStage.Settled:
                await PublishSettledAsync(e.LifecycleId, ct);
                break;
            case ConcertStage.Finished:
                await PublishFinishedAsync(e.LifecycleId, ct);
                break;
        }
    }

    private async Task PublishAppliedAsync(int lifecycleId, CancellationToken ct)
    {
        var lifecycle = await lifecycleRepository.GetByIdAsync(lifecycleId);
        if (lifecycle is null) return;
        var applicationId = await lifecycleRepository.GetApplicationIdByLifecycleIdAsync(lifecycleId);
        if (applicationId is null) return;

        await bus.PublishAsync(
            new ConcertApplicationCreatedEvent(lifecycleId, lifecycle.OpportunityId, lifecycle.ArtistId, applicationId.Value),
            ct);
    }

    private async Task PublishAcceptedAsync(int lifecycleId, CancellationToken ct)
    {
        var applicationId = await lifecycleRepository.GetApplicationIdByLifecycleIdAsync(lifecycleId);
        var bookingId = await lifecycleRepository.GetBookingIdByLifecycleIdAsync(lifecycleId);
        if (applicationId is null || bookingId is null) return;

        await bus.PublishAsync(
            new ConcertApplicationAcceptedEvent(lifecycleId, applicationId.Value, bookingId.Value),
            ct);
    }

    private async Task PublishSettledAsync(int lifecycleId, CancellationToken ct)
    {
        var concertId = await lifecycleRepository.GetConcertIdByLifecycleIdAsync(lifecycleId);
        var bookingId = await lifecycleRepository.GetBookingIdByLifecycleIdAsync(lifecycleId);
        if (concertId is null || bookingId is null) return;

        await bus.PublishAsync(
            new ConcertSettledEvent(lifecycleId, concertId.Value, bookingId.Value),
            ct);
    }

    private async Task PublishFinishedAsync(int lifecycleId, CancellationToken ct)
    {
        var concertId = await lifecycleRepository.GetConcertIdByLifecycleIdAsync(lifecycleId);
        if (concertId is null) return;

        await bus.PublishAsync(new ConcertFinishedEvent(lifecycleId, concertId.Value), ct);
    }
}
