using System.Text.Json.Serialization;

namespace Concertable.B2B.Concert.Application.Responses;

/// <summary>
/// How a concert settles, in the manager view. A fixed-fee deal (FlatFee/VenueHire) settles against a gross
/// derivable from the signed terms; a revenue-share deal (DoorSplit/Guarantee Plus) settles against the
/// venue-declared door take and carries the declaration lifecycle alongside its formula.
/// </summary>
[JsonDerivedType(typeof(FixedSettlement), "fixed")]
[JsonDerivedType(typeof(RevenueShareSettlement), "revenueShare")]
internal interface ISettlement;

/// <summary>The payee gross (VAT-inclusive, minor units) fixed by the signed terms.</summary>
internal sealed record FixedSettlement(long GrossMinor) : ISettlement;

/// <summary>The revenue-share formula plus where the door-take declaration has got to.</summary>
internal sealed record RevenueShareSettlement(IPaymentAmount Formula, ISettlementDeclaration Declaration) : ISettlement;

/// <summary>
/// A closed set of the door-take declaration's states: the venue has not declared (<see cref="Undeclared"/>),
/// has declared but the payer has not confirmed the reviewed gross (<see cref="Declared"/>), or the payer has
/// confirmed it (<see cref="Reviewed"/>). Impossible combinations of the old flags can't be represented.
/// </summary>
[JsonDerivedType(typeof(Undeclared), "undeclared")]
[JsonDerivedType(typeof(Declared), "declared")]
[JsonDerivedType(typeof(Reviewed), "reviewed")]
internal interface ISettlementDeclaration;

/// <summary><see cref="WindowOpen"/> is true once the gig has ended and while it still awaits settlement.</summary>
internal sealed record Undeclared(bool WindowOpen) : ISettlementDeclaration;

/// <summary>
/// The venue-declared door take, Concertable's own ticket sales (minor units), and when it was declared —
/// awaiting the payer's review of the exact settlement.
/// </summary>
internal sealed record Declared(
    decimal DoorRevenue,
    long TicketSalesMinor,
    DateTime DeclaredAtUtc) : ISettlementDeclaration;

/// <summary>The declared figures plus the payer-confirmed settlement gross (minor units) and when it was confirmed.</summary>
internal sealed record Reviewed(
    decimal DoorRevenue,
    long TicketSalesMinor,
    DateTime DeclaredAtUtc,
    long GrossMinor,
    DateTime ReviewedAtUtc) : ISettlementDeclaration;
