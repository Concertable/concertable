using Concertable.B2B.Concert.Contracts;
using Concertable.B2B.Privacy.Infrastructure.Services;
using Concertable.B2B.Tenant.Contracts;
using Concertable.B2B.Tenant.Contracts.Enums;
using Moq;

namespace Concertable.B2B.Privacy.UnitTests;

public sealed class ErasureGateTests
{
    private readonly Mock<ITenantModule> tenantModule = new();
    private readonly Mock<IConcertModule> concertModule = new();
    private readonly ErasureGate gate;

    public ErasureGateTests()
    {
        this.gate = new ErasureGate(tenantModule.Object, concertModule.Object);
    }

    [Fact]
    public async Task HasLiveObligationsAsync_NoMemberships_ReturnsFalseWithoutQueryingConcert()
    {
        var subjectId = Guid.NewGuid();
        tenantModule.Setup(m => m.GetMembershipsAsync(subjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await gate.HasLiveObligationsAsync(subjectId);

        Assert.False(result);
        concertModule.Verify(
            m => m.HasLiveObligationsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HasLiveObligationsAsync_MemberTenantHasObligation_ReturnsTrue()
    {
        var subjectId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        tenantModule.Setup(m => m.GetMembershipsAsync(subjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new MembershipDto(tenantId, "Acme", TenantType.Venue, TenantRole.Owner)]);
        concertModule.Setup(m => m.HasLiveObligationsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await gate.HasLiveObligationsAsync(subjectId);

        Assert.True(result);
    }

    [Fact]
    public async Task HasLiveObligationsAsync_MemberTenantWithNoObligation_ReturnsFalse()
    {
        var subjectId = Guid.NewGuid();
        tenantModule.Setup(m => m.GetMembershipsAsync(subjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new MembershipDto(Guid.NewGuid(), "Acme", TenantType.Artist, TenantRole.Owner)]);
        concertModule.Setup(m => m.HasLiveObligationsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await gate.HasLiveObligationsAsync(subjectId);

        Assert.False(result);
    }
}
