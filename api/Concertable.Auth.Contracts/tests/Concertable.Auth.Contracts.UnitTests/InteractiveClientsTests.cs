using Concertable.Auth.Contracts;

namespace Concertable.Auth.Contracts.UnitTests;

public sealed class InteractiveClientsTests
{
    [Fact]
    public void All_CoversEveryEnumMemberExactlyOnce()
    {
        var members = Enum.GetValues<InteractiveClient>();

        Assert.Equal(members.Length, InteractiveClients.All.Count);
        Assert.Equal(members.ToHashSet(), InteractiveClients.All.Select(info => info.Client).ToHashSet());
    }

    [Fact]
    public void Info_EveryMember_ReturnsItsOwnRow()
    {
        foreach (var client in Enum.GetValues<InteractiveClient>())
            Assert.Equal(client, client.Info().Client);
    }

    [Fact]
    public void All_WireIdsAreDistinct()
    {
        var ids = InteractiveClients.All.Select(info => info.Id).ToList();

        Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [InlineData("customer-web", InteractiveClient.CustomerBrowser)]
    [InlineData("customer-mobile", InteractiveClient.CustomerMobile)]
    [InlineData("venue-web", InteractiveClient.VenueBrowser)]
    [InlineData("venue-mobile", InteractiveClient.VenueMobile)]
    [InlineData("artist-web", InteractiveClient.ArtistBrowser)]
    [InlineData("artist-mobile", InteractiveClient.ArtistMobile)]
    [InlineData("admin", InteractiveClient.Admin)]
    [InlineData("concertable-test", InteractiveClient.E2ETest)]
    public void TryGet_AKnownWireId_ResolvesItsRow(string clientId, InteractiveClient expected)
    {
        var found = InteractiveClients.TryGet(clientId, out var info);

        Assert.True(found);
        Assert.Equal(expected, info.Client);
        Assert.Equal(clientId, info.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("nope")]
    [InlineData("Customer-Web")]
    [InlineData("customer_web")]
    public void TryGet_AnUnknownWireId_ReturnsFalse(string clientId)
    {
        Assert.False(InteractiveClients.TryGet(clientId, out _));
    }

    [Theory]
    [InlineData("customer-web", AuthParty.Customer)]
    [InlineData("customer-mobile", AuthParty.Customer)]
    [InlineData("venue-web", AuthParty.Venue)]
    [InlineData("venue-mobile", AuthParty.Venue)]
    [InlineData("artist-web", AuthParty.Artist)]
    [InlineData("artist-mobile", AuthParty.Artist)]
    [InlineData("admin", AuthParty.Admin)]
    public void TryGet_APartyClient_CarriesThatParty(string clientId, AuthParty expected)
    {
        InteractiveClients.TryGet(clientId, out var info);

        Assert.Equal(expected, info.Party);
    }

    [Fact]
    public void E2ETest_HasNoParty()
    {
        Assert.Null(InteractiveClient.E2ETest.Info().Party);
    }

    [Theory]
    [InlineData(InteractiveClient.VenueBrowser, true)]
    [InlineData(InteractiveClient.ArtistMobile, true)]
    [InlineData(InteractiveClient.Admin, true)]
    [InlineData(InteractiveClient.CustomerBrowser, false)]
    [InlineData(InteractiveClient.E2ETest, false)]
    public void IsB2b_IsTrueForVenueArtistAndAdminOnly(InteractiveClient client, bool expected)
    {
        Assert.Equal(expected, client.Info().IsB2b);
    }

    [Fact]
    public void IsMobile_TracksMobileSchemePresence()
    {
        foreach (var info in InteractiveClients.All)
            Assert.Equal(info.MobileScheme is not null, info.IsMobile);
    }

    [Theory]
    [InlineData(InteractiveClient.CustomerMobile, "concertable-customer://")]
    [InlineData(InteractiveClient.VenueMobile, "concertable-business://")]
    [InlineData(InteractiveClient.ArtistMobile, "concertable-business://")]
    public void MobileScheme_IsTheNativeRedirectScheme(InteractiveClient client, string expected)
    {
        Assert.Equal(expected, client.Info().MobileScheme);
    }

    [Fact]
    public void BrowserAndPartyAgnosticClients_HaveNoMobileScheme()
    {
        Assert.Null(InteractiveClient.CustomerBrowser.Info().MobileScheme);
        Assert.Null(InteractiveClient.Admin.Info().MobileScheme);
        Assert.Null(InteractiveClient.E2ETest.Info().MobileScheme);
    }
}
