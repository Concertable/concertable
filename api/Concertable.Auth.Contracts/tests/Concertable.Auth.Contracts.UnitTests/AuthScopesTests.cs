using Concertable.Auth.Contracts;

namespace Concertable.Auth.Contracts.UnitTests;

public sealed class AuthScopesTests
{
    [Fact]
    public void All_CoversEveryEnumMember()
    {
        var members = Enum.GetValues<AuthScope>();

        Assert.Equal(members.ToHashSet(), AuthScopes.All.ToHashSet());
    }

    [Theory]
    [InlineData(AuthScope.B2BApi, "concertable.b2b.api")]
    [InlineData(AuthScope.CustomerApi, "concertable.customer.api")]
    [InlineData(AuthScope.SearchApi, "concertable.search.api")]
    [InlineData(AuthScope.PaymentWrite, "payment:write")]
    [InlineData(AuthScope.UserClaims, "user:claims")]
    public void Id_IsTheWireScopeString(AuthScope scope, string expected)
    {
        Assert.Equal(expected, scope.Id());
    }

    [Fact]
    public void Id_And_TryGet_RoundTrip()
    {
        foreach (var scope in Enum.GetValues<AuthScope>())
        {
            var found = AuthScopes.TryGet(scope.Id(), out var back);

            Assert.True(found);
            Assert.Equal(scope, back);
        }
    }

    [Fact]
    public void All_WireStringsAreDistinct()
    {
        var ids = AuthScopes.All.Select(scope => scope.Id()).ToList();

        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [InlineData("")]
    [InlineData("payment:read")]
    [InlineData("concertable.b2b")]
    public void TryGet_AnUnknownScope_ReturnsFalse(string scope)
    {
        Assert.False(AuthScopes.TryGet(scope, out _));
    }
}
