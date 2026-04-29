using System.Net;
using Concertable.Shared;

namespace Concertable.Search.IntegrationTests;

[Collection("Integration")]
public class HeaderApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    private record PaginationResponse<T>(IEnumerable<T> Data, int TotalCount, int TotalPages, int PageNumber, int PageSize);

    public HeaderApiTests(ApiFixture fixture) => this.fixture = fixture;
    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    #region GetByAmount

    [Fact]
    public async Task GetByAmount_ShouldReturn200_WithArtists()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/Header/amount/5?headerType=Artist");
        await response.ShouldBe(HttpStatusCode.OK);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var headers = await response.Content.ReadAsync<ArtistHeaderDto[]>();
        Assert.NotNull(headers);
        Assert.Single(headers);
        Assert.Equal("Test Artist", headers[0].Name);
    }

    [Fact]
    public async Task GetByAmount_ShouldReturn200_WithVenues()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/Header/amount/5?headerType=Venue");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var headers = await response.Content.ReadAsync<VenueHeaderDto[]>();
        Assert.NotNull(headers);
        Assert.Single(headers);
        Assert.Equal("Test Venue", headers[0].Name);
    }

    [Fact]
    public async Task GetByAmount_ShouldReturn200_WithConcerts()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/Header/amount/10?headerType=Concert");
        await response.ShouldBe(HttpStatusCode.OK);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var headers = await response.Content.ReadAsync<ConcertHeaderDto[]>();
        Assert.NotNull(headers);
        Assert.NotEmpty(headers);
    }

    [Fact]
    public async Task GetByAmount_ShouldReturn400_WhenNoHeaderType()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/Header/amount/5");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Search

    [Fact]
    public async Task Search_ShouldReturn200_WithPaginatedArtists()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/Header?headerType=Artist&searchTerm=Test");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsync<PaginationResponse<ArtistHeaderDto>>();
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal("Test Artist", result.Data.First().Name);
    }

    [Fact]
    public async Task Search_ShouldReturn200_WithPaginatedVenues()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/Header?headerType=Venue");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsync<PaginationResponse<VenueHeaderDto>>();
        Assert.NotNull(result);
        Assert.Single(result.Data);
        Assert.Equal("Test Venue", result.Data.First().Name);
    }

    [Fact]
    public async Task Search_ShouldReturn200_WithPaginatedConcerts()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/Header?headerType=Concert");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsync<PaginationResponse<ConcertHeaderDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task Search_ShouldReturn200_WithEmptyData_WhenNoMatch()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/Header?headerType=Artist&searchTerm=zzznomatch");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsync<PaginationResponse<ArtistHeaderDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task Search_ShouldReturn200_WithResults_WhenCommaDelimitedGenreIdsContainMatch()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Header?headerType=Artist&genreIds={fixture.SeedData.Rock.Id},{fixture.SeedData.Jazz.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsync<PaginationResponse<ArtistHeaderDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task Search_ShouldReturn200_WithEmptyData_WhenCommaDelimitedGenreIdsHaveNoMatch()
    {
        // Arrange
        var client = fixture.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Header?headerType=Artist&genreIds={fixture.SeedData.Jazz.Id},{fixture.SeedData.Electronic.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsync<PaginationResponse<ArtistHeaderDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Data);
    }

    #endregion
}
