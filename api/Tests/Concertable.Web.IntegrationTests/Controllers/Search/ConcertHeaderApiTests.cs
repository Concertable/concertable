using System.Net;
using Concertable.Shared;
using Concertable.Web.IntegrationTests.Infrastructure;
using Xunit;

namespace Concertable.Web.IntegrationTests.Controllers.Search;

[Collection("Integration")]
public class ConcertHeaderApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ConcertHeaderApiTests(ApiFixture fixture) => this.fixture = fixture;
    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    #region GetPopular

    [Fact]
    public async Task GetPopular_ShouldReturn200_WithPostedConcerts()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/concert/headers/popular");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var concerts = await response.Content.ReadAsync<ConcertHeaderDto[]>();
        Assert.NotNull(concerts);
        Assert.NotEmpty(concerts);
    }

    #endregion

    #region GetFree

    [Fact]
    public async Task GetFree_ShouldReturn200_WithEmptyList_WhenNoPaidConcerts()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/concert/headers/free");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var concerts = await response.Content.ReadAsync<ConcertHeaderDto[]>();
        Assert.NotNull(concerts);
        Assert.Empty(concerts);
    }

    #endregion

    #region GetRecommended

    [Fact]
    public async Task GetRecommended_ShouldReturn401_WhenUnauthenticated()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/concert/headers/recommended");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRecommended_ShouldReturn200_WithConcerts_WhenAuthenticated()
    {
        var client = fixture.CreateClient(fixture.SeedData.Customer);

        var response = await client.GetAsync("/api/concert/headers/recommended");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var concerts = await response.Content.ReadAsync<ConcertHeaderDto[]>();
        Assert.NotNull(concerts);
        Assert.NotEmpty(concerts);
    }

    [Fact]
    public async Task GetRecommended_ShouldReturn200_FilteredByGenre_WhenGenreProvided()
    {
        var client = fixture.CreateClient(fixture.SeedData.Customer);

        var response = await client.GetAsync($"/api/concert/headers/recommended?genreIds={fixture.SeedData.Rock.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var concerts = await response.Content.ReadAsync<ConcertHeaderDto[]>();
        Assert.NotNull(concerts);
        Assert.NotEmpty(concerts);
    }

    [Fact]
    public async Task GetRecommended_ShouldReturn200_WithEmptyList_WhenGenreHasNoMatches()
    {
        var client = fixture.CreateClient(fixture.SeedData.Customer);

        var response = await client.GetAsync($"/api/concert/headers/recommended?genreIds={fixture.SeedData.Jazz.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var concerts = await response.Content.ReadAsync<ConcertHeaderDto[]>();
        Assert.NotNull(concerts);
        Assert.Empty(concerts);
    }

    #endregion
}
