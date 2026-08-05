namespace Concertable.B2B.Tenant.Contracts;

public sealed record TenantConfigurationValues(
    decimal PrsPassThroughRate,
    decimal VatRate,
    int PaymentTermsDays,
    int CancellationNoticeHours);
