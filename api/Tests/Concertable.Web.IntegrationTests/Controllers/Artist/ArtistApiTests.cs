using System.Net;
using Concertable.Application.DTOs;
using Concertable.Web.IntegrationTests.Infrastructure;
using Concertable.Web.Responses;
using Xunit;
using static Concertable.Web.IntegrationTests.Controllers.Artist.ArtistRequestBuilders;

namespace Concertable.Web.IntegrationTests.Controllers.Artist;

[Collection("Integration")]
public class ArtistApiTests : IAsyncLifetime
{
    private readonly ApiFixture fixture;

    public ArtistApiTests(ApiFixture fixture)
    {
        this.fixture = fixture;
    }

    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    #region GetDetailsById

    [Fact]
    public async Task GetDetailsById_ShouldReturn200_WithArtistDetails()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync($"/api/Artist/{fixture.SeedData.Artist.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var artist = await response.Content.ReadAsync<ArtistDetailsResponse>();
        Assert.NotNull(artist);
        Assert.Equal(fixture.SeedData.Artist.Id, artist.Id);
        Assert.Equal("Test Artist", artist.Name);
    }

    [Fact]
    public async Task GetDetailsById_ShouldReturn404_WhenArtistDoesNotExist()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/Artist/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region GetDetailsForCurrentUser

    [Fact]
    public async Task GetDetailsForCurrentUser_ShouldReturn401_WhenUnauthenticated()
    {
        var client = fixture.CreateClient();

        var response = await client.GetAsync("/api/Artist/user");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetDetailsForCurrentUser_ShouldReturn403_WhenNotArtistManager()
    {
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);

        var response = await client.GetAsync("/api/Artist/user");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetDetailsForCurrentUser_ShouldReturn200_WhenArtistExists()
    {
        var client = fixture.CreateClient(fixture.SeedData.ArtistManager);

        var response = await client.GetAsync("/api/Artist/user");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var artist = await response.Content.ReadAsync<ArtistDetailsResponse>();
        Assert.NotNull(artist);
        Assert.Equal("Test Artist", artist.Name);
    }

    [Fact]
    public async Task GetDetailsForCurrentUser_ShouldReturn404_WhenNoArtistExists()
    {
        var client = fixture.CreateClient(fixture.SeedData.ArtistManagerNoArtist);

        var response = await client.GetAsync("/api/Artist/user");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Create

    [Fact]
    public async Task Create_ShouldReturn401_WhenUnauthenticated()
    {
        var client = fixture.CreateClient();
        var request = BuildCreateRequest();

        var response = await client.PostAsync("/api/Artist", await request.ToFormContent());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn403_WhenNotArtistManager()
    {
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);
        var request = BuildCreateRequest();

        var response = await client.PostAsync("/api/Artist", await request.ToFormContent());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn201_WithArtistDto_WhenValidRequest()
    {
        var client = fixture.CreateClient(fixture.SeedData.ArtistManagerNoArtist);
        var request = BuildCreateRequest();

        var response = await client.PostAsync("/api/Artist", await request.ToFormContent());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var artist = await response.Content.ReadAsync<ArtistDto>();
        Assert.NotNull(artist);
        Assert.True(artist.Id > 0);
        Assert.Equal(request.Name, artist.Name);
        Assert.Equal(request.About, artist.About);
        Assert.Equal("Test County", artist.County);
        Assert.Equal("Test Town", artist.Town);
        Assert.EndsWith(".jpg", artist.BannerUrl);
        Assert.True(Guid.TryParse(Path.GetFileNameWithoutExtension(artist.BannerUrl), out _));
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenGeocodingFails()
    {
        var client = fixture.CreateClient(fixture.SeedData.ArtistManagerNoArtist, o => o.UseFailingGeocoding());
        var request = BuildCreateRequest();

        var response = await client.PostAsync("/api/Artist", await request.ToFormContent());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenNameIsEmpty()
    {
        var client = fixture.CreateClient(fixture.SeedData.ArtistManagerNoArtist);
        var request = BuildCreateRequest(name: "");

        var response = await client.PostAsync("/api/Artist", await request.ToFormContent());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update

    [Fact]
    public async Task Update_ShouldReturn401_WhenUnauthenticated()
    {
        var client = fixture.CreateClient();
        var request = BuildUpdateRequest();

        var response = await client.PutAsync($"/api/Artist/{fixture.SeedData.Artist.Id}", await request.ToFormContent());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturn403_WhenNotArtistManager()
    {
        var client = fixture.CreateClient(fixture.SeedData.VenueManager1);
        var request = BuildUpdateRequest();

        var response = await client.PutAsync($"/api/Artist/{fixture.SeedData.Artist.Id}", await request.ToFormContent());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturn403_WhenNotOwner()
    {
        var client = fixture.CreateClient(fixture.SeedData.ArtistManagerNoArtist);
        var request = BuildUpdateRequest();

        var response = await client.PutAsync($"/api/Artist/{fixture.SeedData.Artist.Id}", await request.ToFormContent());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturn404_WhenArtistDoesNotExist()
    {
        var client = fixture.CreateClient(fixture.SeedData.ArtistManager);
        var request = BuildUpdateRequest();

        var response = await client.PutAsync("/api/Artist/99999", await request.ToFormContent());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ShouldReturn200_WithUpdatedArtistDto_WhenValidRequest()
    {
        var client = fixture.CreateClient(fixture.SeedData.ArtistManager);
        var request = BuildUpdateRequest();

        var response = await client.PutAsync($"/api/Artist/{fixture.SeedData.Artist.Id}", await request.ToFormContent());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var artist = await response.Content.ReadAsync<ArtistDto>();
        Assert.NotNull(artist);
        Assert.Equal("Updated Artist", artist.Name);
        Assert.Equal("Updated about", artist.About);
        Assert.Equal("Test County", artist.County);
        Assert.Equal("Test Town", artist.Town);
    }

    [Fact]
    public async Task Update_ShouldReturn400_WhenNameIsEmpty()
    {
        var client = fixture.CreateClient(fixture.SeedData.ArtistManager);
        var request = BuildUpdateRequest(name: "");

        var response = await client.PutAsync($"/api/Artist/{fixture.SeedData.Artist.Id}", await request.ToFormContent());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}
