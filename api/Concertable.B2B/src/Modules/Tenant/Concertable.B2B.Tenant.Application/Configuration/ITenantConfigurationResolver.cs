using Concertable.B2B.Tenant.Contracts;
using Concertable.B2B.Tenant.Domain.ValueObjects;

namespace Concertable.B2B.Tenant.Application.Configuration;

internal interface ITenantConfigurationResolver
{
    TenantConfigurationValues Defaults { get; }
    TenantConfigurationValues Resolve(TenantConfiguration configuration);
}
