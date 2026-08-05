namespace Concertable.B2B.Tenant.Contracts;

public sealed record TenantConfigurationDto
{
    public decimal? PrsPassThroughRate { get; init; }
    public decimal? VatRate { get; init; }
    public int? PaymentTermsDays { get; init; }
    public int? CancellationNoticeHours { get; init; }
}
