using System.Net;
using Application.DTOs;
using Concertable.Web.IntegrationTests.Infrastructure;
using Xunit;
using static Concertable.Web.IntegrationTests.Controllers.Concert.ConcertRequestBuilders;

namespace Concertable.Web.IntegrationTests.Controllers.Concert;

[Collection("Integration")]
public class ConcertApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ConcertApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    #region Post

    [Fact]
    public async Task Post_ShouldReturn401_WhenUnauthenticated()
    {
        var client = fixture.CreateClient();
        var request = BuildPostRequest();

        var response = await client.PutAsync($"/api/Concert/post/{TestConstants.Settled.ConcertId}", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_ShouldReturn403_WhenNotVenueManager()
    {
        var client = fixture.CreateClient(TestConstants.ArtistManager);
        var request = BuildPostRequest();

        var response = await client.PutAsync($"/api/Concert/post/{TestConstants.Settled.ConcertId}", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_ShouldReturn400_WhenApplicationNotSettled()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);
        var request = BuildPostRequest();

        var response = await client.PutAsync($"/api/Concert/post/{TestConstants.AwaitingPayment.ConcertId}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_ShouldReturn200_WithPostedConcert()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);
        var request = BuildPostRequest();

        var response = await client.PutAsync($"/api/Concert/post/{TestConstants.Settled.ConcertId}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var concert = await response.Content.ReadAsync<ConcertDto>();
        Assert.NotNull(concert);
        Assert.Equal(request.Name, concert.Name);
        Assert.Equal(request.About, concert.About);
        Assert.Equal(request.Price, concert.Price);
        Assert.Equal(request.TotalTickets, concert.TotalTickets);
        Assert.NotNull(concert.DatePosted);
    }

    [Fact]
    public async Task Post_ShouldReturn400_WhenAlreadyPosted()
    {
        var client = fixture.CreateClient(TestConstants.VenueManager);
        var request = BuildPostRequest();

        await client.PutAsync($"/api/Concert/post/{TestConstants.Settled.ConcertId}", request);
        var response = await client.PutAsync($"/api/Concert/post/{TestConstants.Settled.ConcertId}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}
