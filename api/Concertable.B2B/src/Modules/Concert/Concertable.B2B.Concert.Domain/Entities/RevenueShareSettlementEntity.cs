using Concertable.B2B.Concert.Domain.Errors;
using Concertable.B2B.DataAccess.Application;
using Concertable.Kernel;

namespace Concertable.B2B.Concert.Domain.Entities;

/// <summary>
/// The settlement record for a revenue-share deal (DoorSplit, Guarantee Plus) — the deal types whose payee
/// gross is only known after the event. A row exists only once the venue has declared the door take; there
/// is none for FlatFee/VenueHire, whose gross is fixed by the signed terms. Kept off
/// <see cref="ConcertEntity"/> so deal-type-specific settlement data does not accrete there as nullables.
/// </summary>
public sealed class RevenueShareSettlementEntity : IIdEntity, IVenueArtistTenantScoped
{
    public int Id { get; private set; }
    public Guid VenueTenantId { get; private set; }
    public Guid ArtistTenantId { get; private set; }
    public int ConcertId { get; private set; }

    /// <summary>
    /// Venue-declared external take the artist's revenue share settles against — external ticketing, box
    /// office and cash on the door. Concertable's own ticket sales are derived separately, not stored here.
    /// </summary>
    public decimal DoorRevenue { get; private set; }
    public DateTime DeclaredAtUtc { get; private set; }

    /// <summary>
    /// The payer's confirmed review of the exact settlement (§4.1) — the frozen gross and when it was
    /// confirmed, as one all-or-nothing value. Null between the venue's declaration and that review; the
    /// completion worker charges against it once set. Re-declaring the door take clears it.
    /// </summary>
    public SettlementReview? Review { get; private set; }

    private RevenueShareSettlementEntity() { }

    /// <summary>Opens the settlement record when the venue first declares the door take.</summary>
    public static Result<RevenueShareSettlementEntity, DoorRevenueDeclarationError> Declare(
        ConcertEntity concert,
        decimal doorRevenue,
        DateTime declaredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(concert);
        if (concert.Booking is not DeferredBooking)
            throw new DomainException("A revenue-share settlement requires a revenue-share (deferred) booking.");
        if (concert.VenueTenantId == Guid.Empty || concert.ArtistTenantId == Guid.Empty)
            throw new DomainException("A revenue-share settlement cannot inherit unresolved concert tenants.");
        if (doorRevenue < 0)
            return new DoorRevenueDeclarationError.NegativeRevenue();

        return new RevenueShareSettlementEntity
        {
            ConcertId = concert.Id,
            VenueTenantId = concert.VenueTenantId,
            ArtistTenantId = concert.ArtistTenantId,
            DoorRevenue = doorRevenue,
            DeclaredAtUtc = declaredAtUtc
        };
    }

    /// <summary>Restates the declared door take before settlement, invalidating any prior payer review.</summary>
    public UnitResult<DoorRevenueDeclarationError> Redeclare(decimal doorRevenue, DateTime declaredAtUtc)
    {
        if (doorRevenue < 0)
            return new DoorRevenueDeclarationError.NegativeRevenue();

        DoorRevenue = doorRevenue;
        DeclaredAtUtc = declaredAtUtc;
        Review = null;
        return new Success();
    }

    /// <summary>Freezes the payer-confirmed review of the exact settlement.</summary>
    public void FreezeReviewedGross(long grossMinor, DateTime reviewedAtUtc)
    {
        if (grossMinor < 0)
            throw new DomainException("A settlement gross cannot be negative.");

        Review = new SettlementReview(grossMinor, reviewedAtUtc);
    }
}
