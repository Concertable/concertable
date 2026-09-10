using Concertable.Auth.Contracts;

namespace Concertable.Auth.Contracts.UnitTests;

public sealed class AuthResourcesTests
{
    [Fact]
    public void All_CoversEveryEnumMemberExactlyOnce()
    {
        var members = Enum.GetValues<AuthResource>();

        Assert.Equal(members.Length, AuthResources.All.Count);
        Assert.Equal(members.ToHashSet(), AuthResources.All.Select(info => info.Resource).ToHashSet());
    }

    [Fact]
    public void Info_EveryMember_ReturnsItsOwnRow()
    {
        foreach (var resource in Enum.GetValues<AuthResource>())
            Assert.Equal(resource, resource.Info().Resource);
    }

    [Theory]
    [InlineData(AuthResource.B2B, "concertable.b2b.api")]
    [InlineData(AuthResource.Customer, "concertable.customer.api")]
    [InlineData(AuthResource.Search, "concertable.search.api")]
    [InlineData(AuthResource.Payment, "concertable.payment.api")]
    public void Audience_IsTheResourceServerAudience(AuthResource resource, string expected)
    {
        Assert.Equal(expected, resource.Info().Audience);
    }

    [Fact]
    public void Payment_AudienceDiffersFromItsScope()
    {
        Assert.Equal("concertable.payment.api", AuthResource.Payment.Info().Audience);
        Assert.Equal("payment:write", AuthScope.PaymentWrite.Id());
    }

    [Fact]
    public void Info_AcceptedScopesAndUserClaims_MatchTheDuendeRegistration()
    {
        var b2b = AuthResource.B2B.Info();
        Assert.Equal([AuthScope.B2BApi], b2b.Scopes.ToArray());
        Assert.Equal(["email"], b2b.UserClaims.ToArray());

        var customer = AuthResource.Customer.Info();
        Assert.Equal([AuthScope.CustomerApi, AuthScope.UserClaims], customer.Scopes.ToArray());
        Assert.Equal(["role", "owner"], customer.UserClaims.ToArray());

        var search = AuthResource.Search.Info();
        Assert.Equal([AuthScope.SearchApi], search.Scopes.ToArray());
        Assert.Empty(search.UserClaims);

        var payment = AuthResource.Payment.Info();
        Assert.Equal([AuthScope.PaymentWrite], payment.Scopes.ToArray());
        Assert.Empty(payment.UserClaims);
    }

    [Fact]
    public void TryGet_ByAudience_RoundTrips()
    {
        foreach (var resource in Enum.GetValues<AuthResource>())
        {
            var found = AuthResources.TryGet(resource.Info().Audience, out var info);

            Assert.True(found);
            Assert.Equal(resource, info.Resource);
        }
    }

    [Fact]
    public void TryGet_AnUnknownAudience_ReturnsFalse()
    {
        Assert.False(AuthResources.TryGet("concertable.unknown.api", out _));
    }

    [Fact]
    public void All_AudiencesAreDistinct()
    {
        var audiences = AuthResources.All.Select(info => info.Audience).ToList();

        Assert.Equal(audiences.Count, audiences.Distinct(StringComparer.Ordinal).Count());
    }
}
