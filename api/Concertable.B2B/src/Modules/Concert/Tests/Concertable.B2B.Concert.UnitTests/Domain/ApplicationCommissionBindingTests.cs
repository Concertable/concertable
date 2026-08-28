using Concertable.B2B.Concert.Domain.Entities;
using Xunit;

namespace Concertable.B2B.Concert.UnitTests.Domain;

public sealed class ApplicationCommissionBindingTests
{
    [Fact]
    public void BindCommission_FirstBinding_RecordsIt()
    {
        var application = CreateApplication();
        var bindingId = Guid.NewGuid();

        application.BindCommission(bindingId);

        Assert.Equal(bindingId, application.CommissionBindingId);
    }

    [Fact]
    public void BindCommission_SameBindingAgain_IsIdempotent()
    {
        var application = CreateApplication();
        var bindingId = Guid.NewGuid();
        application.BindCommission(bindingId);

        application.BindCommission(bindingId);

        Assert.Equal(bindingId, application.CommissionBindingId);
    }

    [Fact]
    public void BindCommission_DifferentBinding_Throws()
    {
        var application = CreateApplication();
        application.BindCommission(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => application.BindCommission(Guid.NewGuid()));
    }

    [Fact]
    public void BindCommission_EmptyBinding_Throws()
    {
        var application = CreateApplication();

        Assert.Throws<InvalidOperationException>(() => application.BindCommission(Guid.Empty));
    }

    private static ApplicationEntity CreateApplication() =>
        StandardApplication.Create(1, 2, DealType.FlatFee, Guid.NewGuid(), Guid.NewGuid());
}
