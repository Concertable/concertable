namespace Concertable.B2B.Tenant.Application.Configuration;

public sealed class TenantConfigurationDefaultsOptions
{
    public const string SectionName = "TenantConfigurationDefaults";

    public decimal PrsPassThroughRate { get; set; }
    public decimal VatRate { get; set; }
    public int PaymentTermsDays { get; set; }
    public int CancellationNoticeHours { get; set; }
}
