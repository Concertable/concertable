using Concertable.B2B.Tenant.Contracts;
using Concertable.B2B.Tenant.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace Concertable.B2B.Tenant.Application.Configuration;

internal sealed class TenantConfigurationResolver : ITenantConfigurationResolver
{
    public TenantConfigurationResolver(IOptions<TenantConfigurationDefaultsOptions> options)
    {
        var defaults = options.Value;
        this.Defaults = new TenantConfigurationValues(
            defaults.PrsPassThroughRate,
            defaults.VatRate,
            defaults.PaymentTermsDays,
            defaults.CancellationNoticeHours);
    }

    public TenantConfigurationValues Defaults { get; }

    public TenantConfigurationValues Resolve(TenantConfiguration configuration) => new(
        configuration.PrsPassThroughRate ?? Defaults.PrsPassThroughRate,
        configuration.VatRate ?? Defaults.VatRate,
        configuration.PaymentTermsDays ?? Defaults.PaymentTermsDays,
        configuration.CancellationNoticeHours ?? Defaults.CancellationNoticeHours);
}
