using System.ComponentModel;
using Concertable.B2B.Concert.Contracts;
using Concertable.B2B.Concert.Domain.Events;
using Concertable.B2B.Concert.Domain.Lifecycle;
using Concertable.B2B.Concert.Domain.ReadModels;
using Concertable.B2B.DataAccess.Application;
using Concertable.Kernel;

namespace Concertable.B2B.Concert.Domain.Entities;

[DisplayName(DisplayNames.Application)]
public abstract class ApplicationEntity : IIdEntity, IVenueArtistTenantScoped, IEventRaiser
{
    public int Id { get; private set; }
    public Guid VenueTenantId { get; private set; }
    public Guid ArtistTenantId { get; private set; }
    internal LifecycleState State { get; private set; } = LifecycleState.Applied;
    internal PaymentVerification PaymentVerification { get; private set; } = PaymentVerification.None;
    public int OpportunityId { get; private set; }
    public int ArtistId { get; private set; }
    public DealType DealType { get; private set; }
    public OpportunityEntity Opportunity { get; set; } = null!;
    public ArtistReadModel Artist { get; set; } = null!;
    public BookingEntity? Booking { get; private set; }
    public Guid? AcceptanceOperationId { get; private set; }
    public Guid? CancellationOperationId { get; private set; }

    /// <summary>
    /// The opaque Payment-issued commission binding fixing the platform rate for this payer commitment.
    /// Null until the payer reaches the binding point; immutable once set (§3.3 — a binding is never rebound).
    /// </summary>
    public Guid? CommissionBindingId { get; private set; }
    public string? FinancialFailureCode { get; private set; }
    public string? FinancialFailureMessage { get; private set; }

    public ESignature ArtistESignature { get; private set; } = null!;
    public string TermsFingerprint { get; private set; } = null!;

    protected ApplicationEntity() { }

    protected ApplicationEntity(
        int artistId,
        int opportunityId,
        DealType dealType,
        Guid venueTenantId,
        Guid artistTenantId)
    {
        if (venueTenantId == Guid.Empty || artistTenantId == Guid.Empty)
            throw new DomainException("An application requires resolved venue and artist tenants.");

        ArtistId = artistId;
        OpportunityId = opportunityId;
        DealType = dealType;
        VenueTenantId = venueTenantId;
        ArtistTenantId = artistTenantId;
    }

    public void Accept(BookingEntity booking) => Booking = booking;

    public Guid BeginAcceptance()
    {
        AcceptanceOperationId ??= Guid.NewGuid();
        FinancialFailureCode = null;
        FinancialFailureMessage = null;
        return AcceptanceOperationId.Value;
    }

    public Guid BeginCancellation()
    {
        if (CancellationOperationId is null || State == LifecycleState.CancellationFailed)
            CancellationOperationId = Guid.NewGuid();
        FinancialFailureCode = null;
        FinancialFailureMessage = null;
        return CancellationOperationId.Value;
    }

    internal void RecordFinancialFailure(string code, string message)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(message))
            throw new DomainException("A financial failure requires a code and message.");

        FinancialFailureCode = code;
        FinancialFailureMessage = message;
    }

    /// <summary>
    /// Records the Payment-issued commission binding at the payer commitment point. Re-supplying the same
    /// binding is idempotent for retry; a different binding is rejected — a commitment is never repriced.
    /// </summary>
    public void BindCommission(Guid commissionBindingId)
    {
        if (commissionBindingId == Guid.Empty)
            throw new DomainException("A commission binding requires a non-empty identifier.");
        if (CommissionBindingId is { } existing && existing != commissionBindingId)
            throw new DomainException("This application is already bound to a different commission binding.");

        CommissionBindingId = commissionBindingId;
    }

    internal void RecordPaymentVerified() => PaymentVerification = PaymentVerification.Verified;

    internal void RecordPaymentFailed() => PaymentVerification = PaymentVerification.Failed;

    public void RecordArtistESignature(ESignature eSignature, string termsFingerprint)
    {
        ArtistESignature = eSignature;
        TermsFingerprint = termsFingerprint;
    }

    internal void Transition(LifecycleState next) => State = next;

    private readonly EventRaiser events = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => events.DomainEvents;
    public void ClearDomainEvents() => events.Clear();

    public void NotifyCounterparty(ApplicationNotification kind)
    {
        var recipient = kind is ApplicationNotification.Applied or ApplicationNotification.Withdrawn
            ? VenueTenantId
            : ArtistTenantId;
        events.Raise(new ApplicationCounterpartyNotifiedDomainEvent(recipient, kind));
    }
}

public sealed class StandardApplication : ApplicationEntity
{
    private StandardApplication() { }

    private StandardApplication(
        int artistId,
        int opportunityId,
        DealType dealType,
        Guid venueTenantId,
        Guid artistTenantId)
        : base(artistId, opportunityId, dealType, venueTenantId, artistTenantId) { }

    public static StandardApplication Create(
        int artistId,
        int opportunityId,
        DealType dealType,
        Guid venueTenantId,
        Guid artistTenantId) =>
        new(artistId, opportunityId, dealType, venueTenantId, artistTenantId);
}

public sealed class PrepaidApplication : ApplicationEntity
{
    public string PaymentMethodId { get; private set; } = null!;

    private PrepaidApplication() { }

    private PrepaidApplication(
        int artistId,
        int opportunityId,
        DealType dealType,
        string paymentMethodId,
        Guid venueTenantId,
        Guid artistTenantId)
        : base(artistId, opportunityId, dealType, venueTenantId, artistTenantId)
    {
        PaymentMethodId = paymentMethodId;
    }

    public static PrepaidApplication Create(
        int artistId,
        int opportunityId,
        DealType dealType,
        string paymentMethodId,
        Guid venueTenantId,
        Guid artistTenantId) =>
        new(artistId, opportunityId, dealType, paymentMethodId, venueTenantId, artistTenantId);
}
