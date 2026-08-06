using Concertable.B2B.Concert.Application.Workflow;
using Concertable.B2B.Concert.Domain.Entities;
using Concertable.B2B.Concert.Infrastructure;
using Concertable.B2B.Concert.Infrastructure.Services.Workflow.Executors;
using Concertable.B2B.Tenant.Contracts;
using Concertable.Kernel.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Concertable.B2B.Concert.UnitTests.Workflow.Executors;

public sealed class CancelExecutorTests
{
    private static readonly DateTime Now = new(2026, 8, 5, 12, 0, 0, DateTimeKind.Utc);
    private readonly Mock<IConcertRepository> concertRepository = new();
    private readonly Mock<ILifecycleTransitioner> transitioner = new();
    private readonly Mock<ITenantModule> tenantModule = new();
    private readonly FakeTimeProvider timeProvider = new(new DateTimeOffset(Now));
    private readonly CancelExecutor executor;

    public CancelExecutorTests()
    {
        this.executor = new CancelExecutor(
            transitioner.Object,
            Mock.Of<IConcertWorkflowFactory>(),
            Mock.Of<IDealResolver>(),
            this.concertRepository.Object,
            tenantModule.Object,
            timeProvider,
            Mock.Of<ILogger<CancelExecutor>>());
    }

    [Fact]
    public async Task CancelAsync_VenueNoticeDeadlineHasPassed_RejectsWithoutTransition()
    {
        var concert = CreateConcert(Now.AddHours(24), Now.AddHours(27));
        concertRepository
            .Setup(r => r.GetByIdWithBookingAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concert);
        tenantModule
            .Setup(t => t.GetConfigurationAsync(concert.VenueTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantConfigurationValues(0.042m, 0.20m, 0, 48));

        var result = await executor.CancelAsync(42);

        Assert.True(result.IsFailed);
        Assert.Contains("cancellation notice deadline", result.Errors.Single().Message);
        transitioner.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CancelAsync_CallerCancellation_Rethrows()
    {
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();
        var cancellationToken = cancellationSource.Token;
        this.concertRepository
            .Setup(r => r.GetByIdWithBookingAsync(It.IsAny<int>(), cancellationToken))
            .Returns(Task.FromCanceled<ConcertEntity?>(cancellationToken));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => this.executor.CancelAsync(42, cancellationToken));
    }

    private static ConcertEntity CreateConcert(DateTime start, DateTime end)
    {
        var concert = ConcertEntity.CreateDraft(
            1,
            2,
            3,
            new DateRange(start, end),
            "Test concert",
            "About",
            DealType.DoorSplit,
            []);
        concert.VenueTenantId = Guid.NewGuid();
        concert.ArtistTenantId = Guid.NewGuid();
        concert.Booking = StandardBooking.Create(17, DealType.DoorSplit);
        return concert;
    }
}
