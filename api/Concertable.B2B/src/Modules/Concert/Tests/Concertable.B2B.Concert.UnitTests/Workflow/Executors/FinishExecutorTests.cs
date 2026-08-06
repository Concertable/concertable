using Concertable.B2B.Concert.Application.Interfaces;
using Concertable.B2B.Concert.Application.Workflow;
using Concertable.B2B.Concert.Application.Workflow.Executors;
using Concertable.B2B.Concert.Domain.Entities;
using Concertable.B2B.Concert.Infrastructure;
using Concertable.B2B.Concert.Infrastructure.Services.Workflow.Executors;
using Concertable.B2B.Tenant.Contracts;
using Concertable.Kernel.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Concertable.B2B.Concert.UnitTests.Workflow.Executors;

public sealed class FinishExecutorTests
{
    private static readonly DateTime Now = new(2026, 8, 5, 12, 0, 0, DateTimeKind.Utc);
    private readonly Mock<IConcertRepository> concertRepository = new();
    private readonly Mock<ILifecycleTransitioner> transitioner = new();
    private readonly Mock<ISettlementPayeeResolver> settlementPayeeResolver = new();
    private readonly Mock<ITicketPayeeResolver> ticketPayeeResolver = new();
    private readonly Mock<ITenantModule> tenantModule = new();
    private readonly FakeTimeProvider timeProvider = new(new DateTimeOffset(Now));
    private readonly FinishExecutor executor;

    public FinishExecutorTests()
    {
        this.executor = new FinishExecutor(
            transitioner.Object,
            Mock.Of<IConcertWorkflowFactory>(),
            Mock.Of<IDealResolver>(),
            this.concertRepository.Object,
            settlementPayeeResolver.Object,
            ticketPayeeResolver.Object,
            Mock.Of<IInvoiceIssuer>(),
            tenantModule.Object,
            Mock.Of<ISelfBillingAgreementGate>(),
            timeProvider,
            Mock.Of<ILogger<FinishExecutor>>());
    }

    [Fact]
    public async Task FinishAsync_SupplierPaymentTermsHaveNotElapsed_DefersWithoutTransition()
    {
        var supplierTenantId = Guid.NewGuid();
        var concert = CreateConcert(Now.AddDays(-7).AddHours(-3), Now.AddDays(-7));
        concertRepository
            .Setup(r => r.GetByIdWithBookingAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concert);
        settlementPayeeResolver.Setup(r => r.ResolveTenantId(concert)).Returns(supplierTenantId);
        tenantModule
            .Setup(t => t.GetConfigurationAsync(supplierTenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantConfigurationValues(0.042m, 0.20m, 14, 0));

        var result = await executor.FinishAsync(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(SettlementOutcome.DeferredPendingPaymentTerms, result.Value);
        transitioner.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task FinishAsync_CallerCancellation_Rethrows()
    {
        using var cancellationSource = new CancellationTokenSource();
        await cancellationSource.CancelAsync();
        var cancellationToken = cancellationSource.Token;
        this.concertRepository
            .Setup(r => r.GetByIdWithBookingAsync(It.IsAny<int>(), cancellationToken))
            .Returns(Task.FromCanceled<ConcertEntity?>(cancellationToken));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => this.executor.FinishAsync(42, cancellationToken));
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
