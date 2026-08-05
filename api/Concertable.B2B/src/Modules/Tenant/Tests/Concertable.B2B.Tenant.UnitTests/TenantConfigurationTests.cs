using Concertable.B2B.Tenant.Domain.ValueObjects;
using Concertable.Kernel;

namespace Concertable.B2B.Tenant.UnitTests;

public sealed class TenantConfigurationTests
{
    [Fact]
    public void Constructor_ValidOverrides_StoresValues()
    {
        var configuration = new TenantConfiguration(0.042m, 0.20m, 30, 72);

        Assert.Equal(0.042m, configuration.PrsPassThroughRate);
        Assert.Equal(0.20m, configuration.VatRate);
        Assert.Equal(30, configuration.PaymentTermsDays);
        Assert.Equal(72, configuration.CancellationNoticeHours);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Constructor_PrsRateOutsideRange_Throws(double value)
    {
        Assert.Throws<DomainException>(() => new TenantConfiguration((decimal)value, null, null, null));
    }

    [Fact]
    public void Empty_HasNoOverrides()
    {
        var configuration = TenantConfiguration.Empty;

        Assert.Null(configuration.PrsPassThroughRate);
        Assert.Null(configuration.VatRate);
        Assert.Null(configuration.PaymentTermsDays);
        Assert.Null(configuration.CancellationNoticeHours);
    }
}
